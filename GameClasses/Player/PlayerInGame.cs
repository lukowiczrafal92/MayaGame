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
        public Dictionary<int, int> scoreSources = new Dictionary<int, int>();
        public Dictionary<int, int> actionsMade = new Dictionary<int, int>();
        public Dictionary<int, int> playerLogTypes = new Dictionary<int, int>();
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

        public void LogAction(ActionTypes action)
        {
            int iKey = (int) action;
            if(actionsMade.ContainsKey(iKey))
                actionsMade[iKey] += 1;
            else
                actionsMade.Add(iKey, 1);
        }
        public void LogPlayerData(PlayerLogTypes plt, int iAmount = 1)
        {
            int iKey = (int) plt;
            if(playerLogTypes.ContainsKey(iKey))
                playerLogTypes[iKey] += iAmount;
            else
                playerLogTypes.Add(iKey, iAmount);
        }
        public void ChangeScoreSource(ScorePointType scorePointType, int iAmount)
        {
            int iKey = (int) scorePointType;
            if(scoreSources.ContainsKey(iKey))
                scoreSources[iKey] += iAmount;
            else
                scoreSources.Add(iKey, iAmount);
        }
        public int GetScoreFromSource(ScorePointType scorePointType)
        {
            int iKey = (int) scorePointType;
            if(scoreSources.ContainsKey(iKey))
                return scoreSources[iKey];
           
           return 0;
        }
        public int GetPlayerLogData(PlayerLogTypes ppt)
        {
            int iKey = (int) ppt;
            if(playerLogTypes.ContainsKey(iKey))
                return playerLogTypes[iKey];
           
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