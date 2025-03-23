using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Models
{
    public class PlayerAngleBoard
    {
        public List<PlayerAngleBoardTile> Angles = new List<PlayerAngleBoardTile>();

        public PlayerAngleBoard()
        {
            var h = GameDataManager.GetAngleBoardTiles();
            foreach(var b in h)
                Angles.Add(new PlayerAngleBoardTile(){dbInfo = b});
        }

        public PlayerAngleBoardTile GetAngleById(int id)
        {
            return Angles.FirstOrDefault(a => a.dbInfo.Id == id);
        }

        public PlayerAngleBoardTile GetMirroredAngleById(int id)
        {
            PlayerAngleBoardTile p = GetAngleById(id);
            int mAngle = (p.dbInfo.Angle + 180) % 360;
            return Angles.FirstOrDefault(a => a.dbInfo.Distance == p.dbInfo.Distance && a.dbInfo.Angle == mAngle);
        }

        public PlayerAngleBoardTile GetAngleByXY(int x, int y)
        {
            return Angles.FirstOrDefault(a => a.dbInfo.X == x && a.dbInfo.Y == y);
        }
        
        public PlayerAngleBoardTile GetPlayerAngleBoardByAngleDistance(int iDistance, int iAngle)
        {
            return Angles.FirstOrDefault(h => h.dbInfo.Angle == iAngle && h.dbInfo.Distance == iDistance);
        }
    }
    public class PlayerAngleBoardTile
    {
        public bool bChecked = false;
        public required AngleBoardTile dbInfo;        
    }
}
