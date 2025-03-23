using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class ActionCardManager
    {
        private readonly GameContext _gameContext;
        private List<ActionCard> actionCards = new List<ActionCard>();
        public ActionCardManager(GameContext gameContext)
        {
            _gameContext = gameContext;
        }
        public ActionCardManager(GameContext gameContext, List<ActionCard> cards)
        {
            _gameContext = gameContext;
            actionCards = cards;
        }

        public List<ActionCard> GetActionCards()
        {
            return actionCards;
        }
        public void CreateActionCardDeck()
        {
            actionCards.Clear();
            int iNumPlayers = _gameContext.PlayerManager.Players.Count;
            foreach(var p in GameDataManager.GetActionCards())
            {
                if(p.StartPerPlayer > 0)
                {
                    int toAdd = p.StartPerPlayer * iNumPlayers;
                    for(int i = 0; i < toAdd; i++)
                        actionCards.Add(new ActionCard(){Id = p.Id, GameIndex = -1});
                }
            }
            foreach(var tile in _gameContext.BoardManager.Tiles)
            {
                if((tile.dbData.TileTypeId == 3) && (tile.dbData.NumPlayers <= iNumPlayers))
                {
                    actionCards.Add(new ActionCard(){Id = 1, GameIndex = -1, LocationId = tile.dbData.Id});
                    actionCards.Add(new ActionCard(){Id = 1, GameIndex = -1, LocationId = tile.dbData.Id});
                }
            }

            var random = new Random();
            actionCards = actionCards
                .OrderBy(p => random.Next())
                .ToList();

            int iGameIndex = 1;
            foreach(var c in actionCards)
            {
                c.GameIndex = iGameIndex;
                iGameIndex++;
            }
        }

        public bool IsThereNeedToSelectActionCards()
        {
            return _gameContext.EraEffectManager.CurrentAgeCardId != 5;
        }
        public int GetNumCardsPerPlayer()
        {
            if(_gameContext.EraEffectManager.CurrentAgeCardId == 5)
                return 5;
            
            if(_gameContext.EraEffectManager.CurrentAgeCardId == 12)
                return 17;

            return 7;
        }

        public ActionCard GetActionCardFromTopDeck()
        {
            ActionCard rtn = actionCards[actionCards.Count - 1];
            actionCards.RemoveAt(actionCards.Count - 1);
            return rtn;
        }
        public void DistributeCards()
        {
            bool bOnlyForEmptyHands = _gameContext.EraEffectManager.CurrentAgeCardId == 12;

            int iCardsPerPlayer = GetNumCardsPerPlayer();
            foreach(var p in _gameContext.PlayerManager.Players)
            {
                if(!bOnlyForEmptyHands || (p.ReserveActionCards.Count == 0))
                {
                    int iToAdd = iCardsPerPlayer - p.ReserveActionCards.Count;
                    for(int i = 0; i < iToAdd; i++)
                        p.ReserveActionCards.Add(GetActionCardFromTopDeck());

                    if(p.ReserveActionCards.Count == 5)
                    {
                        p.HandActionCards.AddRange(p.ReserveActionCards);
                        p.ReserveActionCards.Clear();
                    }

                    SendPlayerActionCardData(p);
                }
            }
        }

        public void ClearAllPlayersActionCards()
        {
            foreach(var p in _gameContext.PlayerManager.Players)
            {
                p.HandActionCards.Clear();
                p.ReserveActionCards.Clear();
            }
        }

        public ActionCardsPlayerData GetPlayerActioncCardsData(PlayerInGame p)
        {
            return new ActionCardsPlayerData
            {
                Player = p.Id,
                Reserve = p.ReserveActionCards,
                Hand = p.HandActionCards
            };
        }

        public void SendPlayerActionCardData(PlayerInGame p)
        {
            ActionCardsPlayerData nData = GetPlayerActioncCardsData(p);
            _gameContext.EventManager.Broadcast("ActionCardsGivenToPlayer", ref nData);
        }
    }
}
