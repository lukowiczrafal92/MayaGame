using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class PlayerLuxury
    {
        public int Id {get; set;} = -1;
        public int Amount {get; set;} = 0;
    }
    public class PlayerLuxuries
    {
        public List<PlayerLuxury> Luxuries = new List<PlayerLuxury>();
        public PlayerLuxuries()
        {
          foreach(var db in GameDataManager.GetLuxuries())
          {
            Luxuries.Add(new PlayerLuxury(){Id = db.Id});
          }
        }
        public PlayerLuxury GetLuxuryById(int id)
        {
          return Luxuries.Find(d => d.Id == id);
        }

        public int GetNumDifferentResources()
        {
            int iDif = 0;
            foreach(var l in Luxuries)
            {
              if(l.Amount > 0)
                iDif++;
            }
            return iDif;
        }
    }
}
