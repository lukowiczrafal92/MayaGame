using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class PlayerDeity
    {
        public int Id {get; set;} = -1;
        public int Level {get; set;} = 0;
        public int Resource {get; set;} = -1;
        public int Luxury {get; set;} = -1;
        public int TieBreaker {get; set;} = 0;
    }
    public class PlayerDeities
    {
        public List<PlayerDeity> Deities = new List<PlayerDeity>();
        public PlayerDeities()
        {
          foreach(var db in GameDataManager.GetDeities())
          {
            Deities.Add(new PlayerDeity(){Id = db.Id, Resource = db.Resource, Luxury = db.LuxuryId});
          }
        }
        public PlayerDeity GetDeityById(int id)
        {
          return Deities.FirstOrDefault(d => d.Id == id);
        }

        public int GetDeitiesAtLevelOrGreater(int iLevel)
        {
          int iCount = 0;
          foreach(var deity in Deities)
          {
            if(deity.Level >= iLevel)
              iCount++;
          }
          return iCount;
        }

        public bool HasTwoDeitiesWithDifferentLevels()
        {
            int iLevelCheck = -1;
            foreach(var deity in Deities)
            {
              if(deity.Level != iLevelCheck)
              {
                if(iLevelCheck == -1)
                  iLevelCheck = deity.Level;
                else
                  return true;
              }
            }
          return false;
        }
        public PlayerDeity GetDeityByResource(int resid)
        {
          return Deities.FirstOrDefault(d => d.Resource == resid);
        }
    }
}
