namespace BoardGameBackend.Models
{
    public class Lobby
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required Guid HostId { get; set; }
        public required string LobbyName { get; set; }
        public List<PlayerInLobby> Players { get; set; } = new List<PlayerInLobby>();
        public string? GameId { get; set; }
    }

    public class LobbyInfo
    {
        public List<Guid> Players { get; set; } = new List<Guid>();
        public Guid HostId { get; set; }
        public bool GameHasStarted { get; set; }
    }

    public class LobbyManagerInfo
    {
        public required Lobby Lobby { get; set; }
        public StartGameModel StartGameModel { get; set; } = new StartGameModel();
    }

    public class CreateLobbyDto
    {
        public required string LobbyName { get; set; } // User who is subscribing    
    }

    public class JoinLobbyDto
    {
        public required string PlayerName { get; set; } // User who is subscribing    
        public required string LobbyId { get; set; } // User who is subscribing       
    }

    public class LeaveLobbyDto
    {
        public required string PlayerName { get; set; } // User who is subscribing    
        public required string LobbyId { get; set; } // User who is subscribing       
    }

    public class DestroyLobbyDto
    {
        public required string PlayerName { get; set; } // User who is subscribing    
        public required string LobbyId { get; set; } // User who is subscribing       
    }

    public class SendLobbyMessageDto
    {// User who is subscribing       
        public required string Message { get; set; }
    }

    public class LobbyJoinResult
    {
        public Lobby? Lobby { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Success => Lobby != null && string.IsNullOrEmpty(ErrorMessage);
    }



}