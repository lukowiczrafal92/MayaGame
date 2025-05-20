using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class StolicaCardsManager
    {
        List<StolicaCard> _availablepool = new List<StolicaCard>();
        private readonly GameContext _gameContext;

        public StolicaCardsManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            int numcards = gameContext.PlayerManager.Players.Count + 1;
            List<StolicaCard> _deck = new List<StolicaCard>();
            foreach(var db in GameDataManager.GetStolice())
            {
                _deck.Add(new StolicaCard(){dbInfo = db});
            }
            Random rng = new Random();
            _deck = _deck.OrderBy(m => rng.Next()).ToList();
            for(int i = 0; i < numcards; i++)
                _availablepool.Add(_deck[i]);
        }
        public StolicaCardsManager(GameContext gameContext, List<int> frb)
        {
            _gameContext = gameContext;
            foreach(var id in frb)
                _availablepool.Add(new StolicaCard(){dbInfo = GameDataManager.GetStolicaById(id)});
        }

        public StolicaCard? GetCardById(int id)
        {
            return _availablepool.FirstOrDefault(s => s.dbInfo.Id == id);
        }

        public void PlayerAcquireCard(PlayerInGame player, int cardid)
        {
            var card = GetCardById(cardid);
            if(card == null)
                return;

            if(_availablepool.Count > 2)
                _availablepool.Remove(card);
            else
                _availablepool.Clear();

            player.Stolica = card.dbInfo.Id;
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){DataType = PlayerBasicSetDataType.CapitalCard, Player = player.Id, Value1 = card.dbInfo.Id, Value2 = 1});
        
            if(card.dbInfo.ConverterId != -1)
                _gameContext.PlayerManager.AddResourceConverter(player, card.dbInfo.ConverterId);
        }

        public List<int> GetFullStolicaBackup()
        {
            List<int> lh = new List<int>();
            foreach(var card in _availablepool)
                lh.Add(card.dbInfo.Id);

            return lh;
        }
    }
}
