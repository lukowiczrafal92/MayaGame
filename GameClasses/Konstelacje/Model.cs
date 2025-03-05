namespace BoardGameBackend.Models
{
    public class KonstelacjaGameData
    {
        public required int Id {get; set;}
        public required List<int> Angles {get; set;}
    }
    public class LuxuryGameData
    {
        public required int Id {get; set;}
    }

    public class Konstelacja
    {
        public required KonstelacjaGameData dbInfo {get; set;}
        public required KonstelacjaSendInfo sendInfo {get; set;}
    }

    public class KonstelacjaSendInfo
    {
        public required int Id {get; set;}
        public Guid PlayerId {get; set;} = Guid.Empty;
    }
}
