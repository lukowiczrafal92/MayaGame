namespace BoardGameBackend.Models
{
    public class Player
    {
        public required  Guid Id {get; set;}
        public required string Name { get; set; }
    }

    public class PlayerInLobby
    {
        public required bool IsConnected { get; set; }
        public required  Guid Id {get; set;}
        public required string Name { get; set; }
    }
}
