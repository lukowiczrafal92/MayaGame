using BoardGameBackend.GameData;
using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class EventsInGameEffectManager
    {
        private readonly GameContext _gameContext;
        public EventsInGameEffectManager(GameContext gameContext)
        {
            _gameContext = gameContext;
        }
        
        public void Trigger(int iKey, GameEventSendData ge)
        {
            switch(iKey)
            {
                case 1: case 2: case 21: case 22: case 23:
                    BoardRotationEffect(ge);
                    break;
                case 3: case 4: case 5: case 6: case 7:
                    HeavyRespecEffect(ge);
                    break;
                case 8:
                    EndEraCapitalPoints(ge);
                    break;
                case 9:
                    EndEraWarfarePoints(ge);
                    break;
                case 10:
                    EndEraLuxuryPoints(ge);
                    break;
                case 11:
                    RerollRulers(ge);
                    break;
                case 12: case 24: case 25: case 26: case 27: case 28:
                    WrathOfGodEffect(ge);
                    break;
                case 13:
                    FamineEffect(ge);
                    break;
                case 14: case 15: case 16: case 17: case 18: case 19:
                    ScoreForLuxury(ge);
                    break;
                case 20:
                    ScoreForCityWithStelae(ge);
                    break;
                default:
                    Console.WriteLine("Missing event handler for " + iKey.ToString());
                    break;
            }
        }

        public void WrathOfGodEffect(GameEventSendData ge)
        {
            var dbinfo = GameDataManager.GetEventById(ge.IntValue1);
            foreach(var p in _gameContext.PlayerManager.Players)
                _gameContext.PlayerManager.ReduceDeityLevel(p, dbinfo.EffectVal1);
        }

        public void FamineEffect(GameEventSendData ge)
        {
            foreach(var p in _gameContext.PlayerManager.Players)
            {
                int setresource = 0;
                foreach(var res in p.PlayerResources.Resources)
                {
                    if(res.Amount > 0)
                        _gameContext.PlayerManager.SetResourceAmount(p, res.Id, 0);
                }
            }
        }

        public void EndEraCapitalPoints(GameEventSendData ge)
        {
           ge.gameEventPlayerTable = _gameContext.ScorePointsManager.EndEraCapitalPoints(ScorePointType.ErasAndEvents);
        }
        public void EndEraWarfarePoints(GameEventSendData ge)
        {
           ge.gameEventPlayerTable = _gameContext.ScorePointsManager.EndEraWarfarePoints(ScorePointType.ErasAndEvents);
        }
        public void EndEraLuxuryPoints(GameEventSendData ge)
        {
           ge.gameEventPlayerTable = _gameContext.ScorePointsManager.EndEraLuxuryPoints(ScorePointType.ErasAndEvents);
        }

        public void ScoreForLuxury(GameEventSendData ge)
        {
            var dbinfo = GameDataManager.GetEventById(ge.IntValue1);
            int luxuryId = dbinfo.EffectVal1;
            ge.gameEventPlayerTable = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.SimpleScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var lrow = new PlayerTableRow(){Player = p.Id};
                int iScore = p.PlayerLuxuries.GetLuxuryById(luxuryId).Amount;
                lrow.Value1 = iScore;
                ge.gameEventPlayerTable.ListPlayerRows.Add(lrow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScore, ScorePointType.ErasAndEvents);
            }
        }

        public void RerollRulers(GameEventSendData ge)
        {
            _gameContext.RulerCardsManager.OnAgeStart();
        }
        public void ScoreForCityWithStelae(GameEventSendData ge)
        {
            ge.gameEventPlayerTable = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.SimpleScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var lrow = new PlayerTableRow(){Player = p.Id};
                int iScore = 0;
                foreach(var tile in _gameContext.BoardManager.Tiles)
                {
                    if(tile.gameData.OwnerId == p.Id && tile.gameData.RulerStelae != -1)
                        iScore++;
                }
                lrow.Value1 = iScore;
                ge.gameEventPlayerTable.ListPlayerRows.Add(lrow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScore, ScorePointType.ErasAndEvents);
            }
        }
        public void HeavyRespecEffect(GameEventSendData ge)
        {
            var dbinfo = GameDataManager.GetEventById(ge.IntValue1);
            int resourceId = dbinfo.EffectVal1;
            foreach(var p in _gameContext.PlayerManager.Players)
            {
                int setresource = 0;
                foreach(var res in p.PlayerResources.Resources)
                {
                    if(res.Amount > 0)
                    {
                        if(res.Id != resourceId)
                        {
                                setresource += res.Amount;
                                _gameContext.PlayerManager.SetResourceAmount(p, res.Id, 0);
                        }
                        else
                            setresource += res.Amount;

                    }
                }

                if(setresource > GameConstants.MAX_RESOURCE_STORAGE)
                    setresource = GameConstants.MAX_RESOURCE_STORAGE;
                
                _gameContext.PlayerManager.SetResourceAmount(p, resourceId, setresource);
            }
        }

        public void BoardRotationEffect(GameEventSendData ge)
        {
            var dbinfo = GameDataManager.GetEventById(ge.IntValue1);
            int rotateval = dbinfo.EffectVal1;
            foreach(var player in _gameContext.PlayerManager.Players)
            {
                player.VisionAngle = (player.VisionAngle + rotateval + 6) % 6;
            }
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = Guid.Empty, DataType = PlayerBasicSetDataType.BoardRotation, Value1 = rotateval});
        }
    }
}