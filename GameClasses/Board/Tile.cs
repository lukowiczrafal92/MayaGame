namespace BoardGameBackend.Models
{
    public class Tile
    {
        public TileGameData dbData;
        public TileGame gameData = new TileGame();
        public bool Jungle = false;
        public Tile(TileGameData gd, int iNumPlayers)
        {
            dbData = gd;
            Jungle = iNumPlayers < gd.NumPlayers;
        }
    }

    public class TileGame
    {
        public Guid OwnerId {get; set;} = Guid.Empty;
        public int Level {get; set;} = 0;
        public int RulerStelae {get; set;} = -1;
    }
}
