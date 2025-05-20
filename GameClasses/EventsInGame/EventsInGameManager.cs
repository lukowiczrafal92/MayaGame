using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class EventsInGameManager
    {
        private readonly GameContext _gameContext;
        public List<int> EventsEraOneRound1 = new List<int>();
        public List<int> EventsEraOneRound2 = new List<int>();
        public List<int> EventsEraTwoRound1 = new List<int>();
        public List<int> EventsEraTwoRound2 = new List<int>();
        public List<int> EventsEraThreeRound1 = new List<int>();
        public List<int> EventsEraThreeRound2 = new List<int>();
        private EventsInGameEffectManager EventsInGameEffectManager;

        public EventsInGameManager(GameContext gameContext, List<List<int>> fullData)
        {
            _gameContext = gameContext;
            EventsInGameEffectManager = new EventsInGameEffectManager(gameContext);
            
            if(!gameContext.GameOptions.AgeCards)
                return;

            EventsEraOneRound1 = fullData[0];
            EventsEraOneRound2 = fullData[1];
            EventsEraTwoRound1 = fullData[2];
            EventsEraTwoRound2 = fullData[3];
            EventsEraThreeRound1 = fullData[4];
            EventsEraThreeRound2 = fullData[5];
        }
        public EventsInGameManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            EventsInGameEffectManager = new EventsInGameEffectManager(gameContext);
            
            if(!gameContext.GameOptions.AgeCards)
                return;

            bool bSplitIntoQuest = gameContext.GameOptions.EventsIntoQuests;
            var deck = new List<EventGameData>();
            var deckQuest = new List<EventGameData>();
            foreach(var dbinfo in GameDataManager.GetEvents())
            {
                if(dbinfo.Enabled)
                {
                    if(bSplitIntoQuest && dbinfo.Quest)
                        deckQuest.Add(dbinfo);
                    else
                        deck.Add(dbinfo);
                }
            }
            Random rng = new Random();
            deck = deck.OrderBy(m => rng.Next()).ToList();
            if(bSplitIntoQuest)
            {
                deckQuest = deckQuest.OrderBy(m => rng.Next()).ToList();
                var questcard = deckQuest.Last();
                deckQuest.Remove(questcard);
                EventsEraOneRound1.Add(questcard.Id);
                questcard = deckQuest.Last();
                deckQuest.Remove(questcard);
                EventsEraOneRound2.Add(questcard.Id);
                questcard = deckQuest.Last();
                deckQuest.Remove(questcard);
                EventsEraTwoRound1.Add(questcard.Id);
                questcard = deckQuest.Last();
                deckQuest.Remove(questcard);
                EventsEraTwoRound2.Add(questcard.Id);
                questcard = deckQuest.Last();
                deckQuest.Remove(questcard);
                EventsEraThreeRound1.Add(questcard.Id);
                questcard = deckQuest.Last();
                deckQuest.Remove(questcard);
                EventsEraThreeRound2.Add(questcard.Id);
            }

            if(_gameContext.EraEffectManager.AgeOneCard != 8)
            {
                var card1 = deck.Last();
                deck.Remove(card1);
                EventsEraOneRound1.Add(card1.Id);
                var card2 = deck.Last();
                deck.Remove(card2);
                EventsEraOneRound2.Add(card2.Id);
                if(_gameContext.EraEffectManager.AgeOneCard == 6)
                {
                    var cardextra1 = deck.FirstOrDefault(dc => dc.GroupType == -1 || dc.GroupType != card1.GroupType);
                    deck.Remove(cardextra1);
                    EventsEraOneRound1.Add(cardextra1.Id);
                    var cardextra2 = deck.FirstOrDefault(dc => dc.GroupType == -1 || dc.GroupType != card2.GroupType);
                    deck.Remove(cardextra2);
                    EventsEraOneRound2.Add(cardextra2.Id);
                }
            }

            if(_gameContext.EraEffectManager.AgeTwoCard != 8)
            {
                var card1 = deck.Last();
                deck.Remove(card1);
                EventsEraTwoRound1.Add(card1.Id);
                var card2 = deck.Last();
                deck.Remove(card2);
                EventsEraTwoRound2.Add(card2.Id);
                if(_gameContext.EraEffectManager.AgeTwoCard == 6)
                {
                    var cardextra1 = deck.FirstOrDefault(dc => dc.GroupType == -1 || dc.GroupType != card1.GroupType);
                    deck.Remove(cardextra1);
                    EventsEraTwoRound1.Add(cardextra1.Id);
                    var cardextra2 = deck.FirstOrDefault(dc => dc.GroupType == -1 || dc.GroupType != card2.GroupType);
                    deck.Remove(cardextra2);
                    EventsEraTwoRound2.Add(cardextra2.Id);
                }
            }

            if(_gameContext.EraEffectManager.AgeThreeCard != 8)
            {
                var card1 = deck.Last();
                deck.Remove(card1);
                EventsEraThreeRound1.Add(card1.Id);
                var card2 = deck.Last();
                deck.Remove(card2);
                EventsEraThreeRound2.Add(card2.Id);
                if(_gameContext.EraEffectManager.AgeThreeCard == 6)
                {
                    var cardextra1 = deck.FirstOrDefault(dc => dc.GroupType == -1 || dc.GroupType != card1.GroupType);
                    deck.Remove(cardextra1);
                    EventsEraThreeRound1.Add(cardextra1.Id);
                    var cardextra2 = deck.FirstOrDefault(dc => dc.GroupType == -1 || dc.GroupType != card2.GroupType);
                    deck.Remove(cardextra2);
                    EventsEraThreeRound2.Add(cardextra2.Id);
                }
            }
        }
        public void RemoveCardIdFrom(ref List<int> listvalue, int cardid)
        {
            if(listvalue.Contains(cardid))
                listvalue.Remove(cardid);
        }
        public void RemoveEffectCardId(int cardid)
        {
            RemoveCardIdFrom(ref EventsEraOneRound1, cardid);
            RemoveCardIdFrom(ref EventsEraOneRound2, cardid);
            RemoveCardIdFrom(ref EventsEraTwoRound1, cardid);
            RemoveCardIdFrom(ref EventsEraTwoRound2, cardid);
            RemoveCardIdFrom(ref EventsEraThreeRound1, cardid);
            RemoveCardIdFrom(ref EventsEraThreeRound2, cardid);
        }
        public List<List<int>> GetFullData()
        {
            List<List<int>> a = new List<List<int>>(){EventsEraOneRound1, EventsEraOneRound2, EventsEraTwoRound1, EventsEraTwoRound2, EventsEraThreeRound1, EventsEraThreeRound2};
            return a;
        }

        public List<int> GetCurrentEventList()
        {
            if(_gameContext.PhaseManager.CurrentEra == 1)
            {
                if(_gameContext.PhaseManager.CurrentRound == 1)
                    return EventsEraOneRound1;

                return EventsEraOneRound2;
            }
            else if(_gameContext.PhaseManager.CurrentEra == 2)
            {
                if(_gameContext.PhaseManager.CurrentRound == 1)
                    return EventsEraTwoRound1;

                return EventsEraTwoRound2;
            }
            if(_gameContext.PhaseManager.CurrentRound == 1)
                return EventsEraThreeRound1;

            return EventsEraThreeRound2;
        }

        public void TriggerEventIfPossible()
        {
            List<int> evenlist = GetCurrentEventList();
            if(evenlist.Count == 0)
                return;
            
            if(evenlist.Count > 1)
                _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.InGameEvent, ActivePlayers = new List<Guid>()});
            int eventid = evenlist[0];
            RemoveEffectCardId(eventid);
            GameEventSendData ge = new GameEventSendData(){gameEventSendType = GameEventSendType.GenericEvent, IntValue1 = eventid};
            EventsInGameEffectManager.Trigger(eventid, ge);
            _gameContext.ActionManager.AddNewGameEvent(ge);
        }
    }
}
