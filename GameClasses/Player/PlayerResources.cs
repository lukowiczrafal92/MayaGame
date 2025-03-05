using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class PlayerResource
    {
        public int Id {get; set;} = -1;
        public int Amount {get; set;} = 0;
        public int Income {get; set;} = 0;
        public int EndGameScore {get; set; } = 0;
        public List<int> Converters = new List<int>();
    }
    public class PlayerResources
    {
        public List<PlayerResource> Resources = new List<PlayerResource>();
        public PlayerResources()
        {
            foreach(var h in GameDataManager.GetResources())
            {
                Resources.Add(new PlayerResource(){Id = h.Id});
            }
        }

        public PlayerResource GetResourceById(int id)
        {
            return Resources.Find(p => p.Id == id);
        }
    }
}
