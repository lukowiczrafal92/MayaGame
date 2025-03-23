using BoardGameBackend.Models;
using MongoDB.Driver;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace BoardGameBackend.Repositories
{
    public class MongoAuthService : IAuthService
    {
        private const string databaseName = "Game";
        private const string collectionName = "User";

        private readonly IMongoCollection<UserModel> usersCollection;
        private readonly FilterDefinitionBuilder<UserModel> filterBuilder = Builders<UserModel>.Filter;
        private readonly IConfiguration configuration;

        //private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public MongoAuthService(IMongoClient mongoClient, IConfiguration configuration){
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            this.configuration = configuration;
            usersCollection = database.GetCollection<UserModel>(collectionName);   
            
            var indexKeysDefinition = Builders<UserModel>.IndexKeys.Ascending(user => user.Username);
            var indexModel = new CreateIndexModel<UserModel>(indexKeysDefinition, new CreateIndexOptions { Unique = true });
            usersCollection.Indexes.CreateOne(indexModel);  
        }

        public async Task<Exception?> SignUpAsync(UserModel userModel){
            try{
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(userModel.Password);
                userModel.Password = passwordHash;
                await usersCollection.InsertOneAsync(userModel);
                return null;
            }
            catch (Exception e){
                Console.WriteLine($"{e}");
                return e; 
            }
        }


        public async Task<AuthenticateData?> LoginAsync(LoginUserDto userLogin)
        {
            var filter =  filterBuilder.Eq(existingUser => existingUser.Username, userLogin.Username);
            var user = await usersCollection.Find(filter).SingleOrDefaultAsync();

            if(user==null) return null;
            bool correctPassword = BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password);
            if(!correctPassword) return null;

            var token = GenerateJwtToken(user);

            var authenticateData = new AuthenticateData(){
                User=user,
                Token=token
            };
            return authenticateData;
        }

        public async Task<UserModel> GetUserById(Guid id)
        {
            var filter =  filterBuilder.Eq(existingUser => existingUser.Id, id);
            var user = await usersCollection.Find(filter).SingleOrDefaultAsync();
            
            return user;
        }

        private string GenerateJwtToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            string secret = configuration.GetSection("TokenSettings:Secret").Value!;
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("id", user.Id.ToString()),
                }),
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}