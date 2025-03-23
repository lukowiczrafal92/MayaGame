using BoardGameBackend.Managers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Repositories
{
    public interface IAuthService
    {
        Task<Exception?> SignUpAsync(UserModel userModel);
        Task<AuthenticateData?> LoginAsync(LoginUserDto userLogin);
        Task<UserModel> GetUserById(Guid id);
    }
    public interface IGameBackupSaver
    {
        void InsertBackup(FullMayabBackup fMayabBackup);
        void UpdateBackup(string gameId, FullGameBackup fGameBackup);
        void DeleteGameId(string gameid);
        void DeleteLobbyId(string lobbyId);
        Task<List<FullMayabBackup>?> GetAllBackupData();
    }

    public class MyHostedService : IHostedService
    {
        private readonly IGameBackupSaver _mySingleton;

        public MyHostedService(IGameBackupSaver mySingleton)
        {
            _mySingleton = mySingleton;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("We try to reload backups starting...");

            LobbyManager.RecreateLobbyGameFromBackups(await _mySingleton.GetAllBackupData());
          //  _mySingleton.DoSomething(); // Logic at startup

        //    return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("MyHostedService stopping...");
            return Task.CompletedTask;
        }
    }
}