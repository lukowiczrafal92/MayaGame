using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Managers;

namespace BoardGameBackend.Models
{

    public class PlayerInGame
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Points { get; set; } = 0;
        public int WarfareScore { get; set; } = 0;
        public int VisionAngle { get; set; } = 0;
        public int CurrentOrder {get; set; } = -1;
        public int IncomingOrder {get; set; } = -1;
        public bool Active {get; set;} = false;
        private Dictionary<ScorePointType, int> scoreSources = new Dictionary<ScorePointType, int>();
        public PlayerAngleBoard PlayerAngleBoard = new PlayerAngleBoard();
        public PlayerResources PlayerResources = new PlayerResources();
        public PlayerLuxuries PlayerLuxuries = new PlayerLuxuries();
        public PlayerDeities PlayerDeities = new PlayerDeities();
        public List<ActionCard> HandActionCards = new List<ActionCard>();
        public List<int> AuraEffects = new List<int>();
        public List<ActionCard> ReserveActionCards = new List<ActionCard>();
        public List<RulerCard> Rulers = new List<RulerCard>();
        public PlayerInGame(Player player)
        {
            Id = player.Id;
            Name = player.Name;
        }
        public void SetVisionAngle(int iVisionAngle)
        {
            VisionAngle = iVisionAngle;
        }

        public void ChangeScoreSource(ScorePointType scorePointType, int iAmount)
        {
            if(scoreSources.ContainsKey(scorePointType))
                scoreSources[scorePointType] += iAmount;
            else
                scoreSources.Add(scorePointType, iAmount);
        }
        public int GetScoreFromSource(ScorePointType scorePointType)
        {
            if(scoreSources.ContainsKey(scorePointType))
                return scoreSources[scorePointType];
           
           return 0;
        }
    }

    public class PlayerViewModelData
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required int VisionAngle { get; set; }
        public int CurrentOrder {get; set; } = -1;
        public int IncomingOrder {get; set; } = -1;
    }
}