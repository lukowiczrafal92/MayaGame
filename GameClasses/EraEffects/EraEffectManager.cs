using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class EraEffectManager
    {
        private readonly GameContext _gameContext;
        private EraEffectTriggersManager EraEffectTriggersManager;
        public int CurrentAgeCardId = -1;
        public int AgeOneCard = -1;
        public int AgeTwoCard = -1;
        public int AgeThreeCard = -1;
        public EraEffectManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            EraEffectTriggersManager = new EraEffectTriggersManager(gameContext);

            if(!gameContext.GameOptions.AgeCards)
                return;

            var deck = new List<int>();
            foreach(var dbinfo in GameDataManager.GetEraEffects())
            {
                if(dbinfo.Enabled)
                    deck.Add(dbinfo.Id);
            }
            Random rng = new Random();
            deck = deck.OrderBy(m => rng.Next()).ToList();

            AgeOneCard = deck[0];
            AgeTwoCard = deck[1];
            AgeThreeCard = deck[2];
        }
        public EraEffectManager(GameContext gameContext, List<int> fullData)
        {
            _gameContext = gameContext;
            EraEffectTriggersManager = new EraEffectTriggersManager(gameContext);

            if(!gameContext.GameOptions.AgeCards)
                return;

            CurrentAgeCardId = fullData[0];
            AgeOneCard = fullData[1];
            AgeTwoCard = fullData[2];
            AgeThreeCard = fullData[3];
        }

        public int GetCurrentAgeEffectId()
        {
            return CurrentAgeCardId;
        }

        public List<int> GetFullData()
        {
            List<int> a = new List<int>(){CurrentAgeCardId, AgeOneCard, AgeTwoCard, AgeThreeCard};
            return a;
        }

        public int GetNextEraCardIdAndClear()
        {
            int iAnswer = -1;
            if(AgeOneCard != -1)
            {
                iAnswer = AgeOneCard;
                AgeOneCard = -1;
            }
            else if(AgeTwoCard != -1)
            {
                iAnswer = AgeTwoCard;
                AgeTwoCard = -1;
            }
            else if(AgeThreeCard != -1)
            {
                iAnswer = AgeThreeCard;
                AgeThreeCard = -1;
            }
            return iAnswer;
        }

        public void TriggerNextEra()
        {
            GameEventSendData ge = new GameEventSendData(){gameEventSendType = GameEventSendType.EraStart};
            _gameContext.ActionManager.AddNewGameEvent(ge);
            int cardid = GetNextEraCardIdAndClear();
            if(cardid != -1)
            {
                CurrentAgeCardId = cardid;
                EraEffectTriggersManager.Trigger(CurrentAgeCardId, 1, true);
            }
            ge.IntValue1 = cardid;
        }

        public int EndCurrentEraEffect()
        {
            int iResponse = CurrentAgeCardId;
            if(CurrentAgeCardId != -1)
            {
                EraEffectTriggersManager.Trigger(CurrentAgeCardId, -1, false);
                CurrentAgeCardId = -1;
            }
            return iResponse;
        }
    }
}
