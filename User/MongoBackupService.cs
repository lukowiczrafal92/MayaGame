using BoardGameBackend.Models;
using MongoDB.Driver;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Repositories
{
    public class MongoBackupService : IGameBackupSaver
    {
        private const string databaseName = "Game";
        private const string collectionName = "MayabBackup";
        private readonly IMongoCollection<FullMayabBackup> backupCollection;
        private readonly FilterDefinitionBuilder<FullMayabBackup> filterBuilder = Builders<FullMayabBackup>.Filter;
        private readonly UpdateDefinitionBuilder<FullMayabBackup> updateBuilder = Builders<FullMayabBackup>.Update;
        private readonly IConfiguration configuration;

        public MongoBackupService(IMongoClient mongoClient, IConfiguration configuration){
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            this.configuration = configuration;
            backupCollection = database.GetCollection<FullMayabBackup>(collectionName);
            DeleteInitial();
        }

        public async void DeleteInitial()
        {
            var filter =  filterBuilder.Where(mayabbackup => mayabbackup.GameVersion != GameConstants.m_iGameVersion);
        //    var filter =  filterBuilder.Where(mayabbackup => true);
            await backupCollection.DeleteManyAsync(filter);  
        }

        public async void DeleteGameId(string gameid)
        {
            var filter =  filterBuilder.Where(mayabbackup => mayabbackup.GameId == gameid);
            await backupCollection.DeleteOneAsync(filter);
        }
        public async void DeleteLobbyId(string lobbyId)
        {
            var filter =  filterBuilder.Where(mayabbackup => mayabbackup.lobbyinfo.Lobby.Id == lobbyId);
            Console.WriteLine("Hello hello hello! " + lobbyId);
            await backupCollection.DeleteOneAsync(filter);
        }
        public async void InsertBackup(FullMayabBackup fMayabBackup)
        {
            Console.WriteLine("Inserting backup: " + fMayabBackup.GameId);
            var filter = filterBuilder.Where(mayabbackup => mayabbackup.GameId == fMayabBackup.GameId);
            var options = new FindOneAndReplaceOptions<FullMayabBackup>
            {
                IsUpsert = true
            };
            await backupCollection.FindOneAndReplaceAsync(filter, fMayabBackup, options);
        }
        public async void UpdateBackup(string gameId, FullGameBackup fGameBackup)
        {
            Console.WriteLine("Updating backup: " + gameId);
            var filter =  filterBuilder.Where(mayabbackup => mayabbackup.GameId == gameId);
            var update = updateBuilder.Set(p => p.FullGameBackup, fGameBackup);
            await backupCollection.UpdateOneAsync(filter, update);
        }

        public async Task<List<FullMayabBackup>?> GetAllBackupData()
        {
            var filter =  filterBuilder.Eq(mayabbackup => mayabbackup.GameVersion, GameConstants.m_iGameVersion);
            var dane = await (await backupCollection.FindAsync(filter)).ToListAsync();
         //   var dane = await (await backupCollection.FindAsync(_ => true)).ToListAsync();
            return dane;
        }
    }
}