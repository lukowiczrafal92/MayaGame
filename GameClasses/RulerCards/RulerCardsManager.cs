using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class RulerCardsManager
    {
        List<RulerCard> _deck = new List<RulerCard>();
        List<RulerCard> _availablepool = new List<RulerCard>();
        private readonly GameContext _gameContext;

        private int[] cPerPlayers = new int[6]{3,4,5,6,7,8};
        public RulerCardsManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            foreach(var ruler in GameDataManager.GetRulers())
            {
                _deck.Add(new RulerCard(){dbInfo = ruler});
            }
            Random rng = new Random();
            _deck = _deck.OrderBy(m => rng.Next()).ToList();
        }
        public void OnAgeStart()
        {
            _availablepool.Clear();
            int iPoolBasedOnPlayers = cPerPlayers[_gameContext.PlayerManager.Players.Count()];
            for(int i = 0; i < iPoolBasedOnPlayers; i++)
            {
                if(_deck.Count() > 0)
                {
                    _availablepool.Add(_deck[_deck.Count() -1]);
                    _deck.RemoveAt(_deck.Count() -1);
                }
                else
                    break;
            }
        }

        private RulerCard? GetRulerCardFromPoolById(int rulerid)
        {
            RulerCard answer;
            if(rulerid == 0)
            {
                answer =_deck[_deck.Count() -1];
                _deck.RemoveAt(_deck.Count() -1);
                return answer;
            }
            else
            {
                foreach(var h in _availablepool)
                {
                    if(h.dbInfo.Id == rulerid)
                    {
                        answer = h;
                        _availablepool.Remove(answer);
                        return answer;
                    }
                }
            }
            return null;
        }

        public void PlayerAcquiredRulerCard(PlayerInGame player, int rulerid, int tileid)
        {
            RulerCard newruler = GetRulerCardFromPoolById(rulerid);
            if(newruler != null)
            {
               player.Rulers.Add(newruler);
               _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){DataType = PlayerBasicSetDataType.RulerCard, Player = player.Id, Value1 = newruler.dbInfo.Id, Value2 = 1});
               _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){DataType = PlayerBasicSetDataType.StelaeToken, Player = player.Id, Value1 = tileid, Value2 = rulerid});
               _gameContext.BoardManager.GetTileById(tileid).gameData.RulerStelae = rulerid;
                // effects and other rewards:
                // Converters for now
                if(newruler.dbInfo.ConverterId != -1)
                    _gameContext.PlayerManager.AddResourceConverter(player, newruler.dbInfo.ConverterId);

                if(newruler.dbInfo.LuxuryId != -1)
                    _gameContext.PlayerManager.ChangeLuxuryAmount(player, newruler.dbInfo.LuxuryId, 1);

                if(newruler.dbInfo.DeityId != -1)
                    player.PlayerDeities.GetDeityById(newruler.dbInfo.DeityId).TieBreaker++;

                if(newruler.dbInfo.InstantResource != -1)
                    _gameContext.PlayerManager.ChangeResourceAmount(player, newruler.dbInfo.InstantResource, newruler.dbInfo.AbilityInfo);

                if(newruler.dbInfo.EndGameResource != -1)
                    player.PlayerResources.GetResourceById(newruler.dbInfo.EndGameResource).EndGameScore += newruler.dbInfo.AbilityInfo;

                if(newruler.dbInfo.AuraEffectId != -1)
                     _gameContext.PlayerManager.AddEffectId(player, newruler.dbInfo.AuraEffectId);
            }
        }

        public bool HasPoolRulerId(int rulerId)
        {
            return (_availablepool.FirstOrDefault(h => h.dbInfo.Id == rulerId) != null);
        }

        public void GetRulersCompleteData(FullRulerData frd)
        {
            frd.DeckAmount = _deck.Count();
            foreach(var h in _availablepool)
                frd.RulerPool.Add(h.dbInfo.Id);

        }
        public List<int> GetRulerPool()
        {
            List<int> nList = new();
            foreach(var h in _availablepool)
                nList.Add(h.dbInfo.Id);

            return nList;
        }
        public int GetRulersDeckAmount()
        {
            return _deck.Count();
        }
    }
}
