namespace BoardGameBackend.Models
{
    public class DeityGameData
    {
        public int Id {get; set;}
        public int Resource {get; set;}
        public int LuxuryId {get; set;}
    }
    public class ResourceGameData
    {
        public int Id {get; set;}
        public int StartingValue {get; set;}
    }

    public class ResourceConverterGameData
    {
        public int Id {get; set;}
        public int FromResource {get; set;}
        public int ToResource {get; set;}
        public int FromValue {get; set;}
        public int ToValue {get; set;}
        public bool Initial {get; set;}
    }
    public class TileTypeGameData
    {
        public int Id {get; set;}
        public required string TileType {get; set;}
    }
    public class AngleBoardTile
    {
        public int Id {get; set;}
        public int X {get; set;}
        public int Y {get; set;}
        public int Angle {get; set;}
        public int Distance {get; set;}
    }
    public class TileGameData
    {
        public int Id {get; set;}
        public int MapX {get; set;}
        public int MapY {get; set;}
        public int TileTypeId {get; set;}
        public int NumPlayers {get; set;}
        public int DeityId {get; set;}
        public int Resource1 {get; set;}
        public int Resource2 {get; set;}
        public int LuxuryId {get; set;}
        public int AdjExtraDeityId {get; set;}
    }
}