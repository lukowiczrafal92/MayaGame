namespace BoardGameBackend.MiddleWare;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BoardGameBackend.Repositories;

public class JwtMiddleware
{
    private readonly RequestDelegate next;
    private readonly IConfiguration configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        this.next = next;
        this.configuration = configuration;
    }

    public async Task Invoke(HttpContext context, IAuthService userService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null) await attachUserToContext(context, token, userService);

        await next(context);
    }

    private async Task attachUserToContext(HttpContext context, string token, IAuthService userService)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            string secret = configuration.GetSection("TokenSettings:Secret").Value!;
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            
            context.Items["User"] = await userService.GetUserById(userId);
        }
        catch (Exception e){
            Console.Write(e);

        }
    }
}