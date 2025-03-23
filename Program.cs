using BoardGameBackend.Hubs;
using BoardGameBackend.MiddleWare;
using BoardGameBackend.Repositories;
using MongoDB.Driver;
using BoardGameBackend.Settings;
using System.Text.Json.Serialization;
using BoardGameBackend.Providers;
using BoardGameBackend.Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using BoardGameBackend.Models;

var builder = WebApplication.CreateBuilder(args);

IConfigurationBuilder configB = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(true);

IConfiguration config = configB.Build();

var mongoDbSettings = config.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;

builder.Services.AddSingleton<IMongoClient>(ServiceProvider =>
{
    return new MongoClient(mongoDbSettings.ConnectionStringSetup);
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
})
.AddJsonOptions(opt =>
     {
         opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
     });

builder.Services.AddSignalR(options =>
{
    options.MaximumParallelInvocationsPerClient = 5;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSingleton<IAuthService, MongoAuthService>();
builder.Services.AddSingleton<IGameBackupSaver, MongoBackupService>();
builder.Services.AddSingleton<IHubContextProvider, HubContextProvider>();
builder.Services.AddHostedService<MyHostedService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("TokenSettings:Secret").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        // This is the key part for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/lobbyhub"))
                {

                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
var app = builder.Build();


var hubContextProvider = app.Services.GetRequiredService<IHubContextProvider>();
var singletonReadBackups = app.Services.GetRequiredService<IGameBackupSaver>();
GameManager.Initialize(hubContextProvider, singletonReadBackups);

//LobbyManager.RecreateLobbyGameFromBackups(await singletonReadBackups.GetAllBackupData());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.MapHub<LobbyHub>("/lobbyhub").RequireAuthorization();
app.Run();
