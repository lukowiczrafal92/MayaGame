using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class EraEffectTriggersManager
    {
        private readonly GameContext _gameContext;
        public EraEffectTriggersManager(GameContext gameContext)
        {
            _gameContext = gameContext;
        }
        
        public void Trigger(int iKey, int iAdd, bool bAdd)
        {
            var dbinfo = GameDataManager.GetEraEffectById(iKey);
            if(dbinfo.GameEffectId != -1)
            {
                foreach(var p in _gameContext.PlayerManager.Players)
                {
                    if(bAdd)
                        _gameContext.PlayerManager.AddEffectId(p, dbinfo.GameEffectId);
                    else
                        _gameContext.PlayerManager.RemoveEffectId(p, dbinfo.GameEffectId);
                }
            }
            switch(iKey)
            {
                case 1:
                    TriggerRainsOfChaac(iAdd, bAdd);
                    break;
                case 9:
                    TriggerCapitalIncome(iAdd, bAdd);
                    break;
                default:
                    break;
            }
        }
        public void TriggerRainsOfChaac(int iAdd, bool bAdd)
        {
            foreach(var p in _gameContext.PlayerManager.Players)
                _gameContext.PlayerManager.ChangeResourceIncome(p, 6, 3 * iAdd);
        }
        public void TriggerCapitalIncome(int iAdd, bool bAdd)
        {
            foreach(var p in _gameContext.PlayerManager.Players)
            {
                var tile = _gameContext.BoardManager.GetCapitalCity(p.Id);
                if(tile != null)
                {
                    _gameContext.PlayerManager.ChangeResourceIncome(p, tile.dbData.Resource1, iAdd);
                    _gameContext.PlayerManager.ChangeResourceIncome(p, tile.dbData.Resource2, iAdd);
                }
            }
        }
    }
}
