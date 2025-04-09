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
                case 29: case 30: case 31: case 32: case 33: case 34:
                    ScoreForDeity(ge);
                    break;
                case 35:
                    AddSpecialistIfZero(ge);
                    break;
                case 36:
                    AddCapitalExpand(ge);
                    break;
                case 37:
                    RemoveCapitalExpand(ge);
                    break;
                case 38:
                    LooseHalfWarfareScore(ge);
                    break;
                case 39:
                    ScoreForCityStack(ge);
                    break;
                case 40: case 41: case 42: case 43: case 44: case 45: case 47: case 50: case 51: case 52: case 53: case 54: case 55:
                    DoQueueExtraPhaseAction(ge);
                    break;
                case 46:
                    ScoreForRulers(ge);
                    break;
                case 48:
                    ScoreForThreeAngles(ge, 90, 210, 330);
                    break;
                case 49:
                    ScoreForThreeAngles(ge, 30, 150, 270);
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
                int burntResources = 0;
                foreach(var res in p.PlayerResources.Resources)
                {
                    if(res.Amount > 0)
                    {
                        burntResources += res.Amount;
                        _gameContext.PlayerManager.SetResourceAmount(p, res.Id, 0);
                    }
                }
                p.LogPlayerData(PlayerLogTypes.SpecialistsBurnt, burntResources);
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
        public void ScoreForDeity(GameEventSendData ge)
        {
            var dbinfo = GameDataManager.GetEventById(ge.IntValue1);
            int deityid = dbinfo.EffectVal1;
            ge.gameEventPlayerTable = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.SimpleScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var lrow = new PlayerTableRow(){Player = p.Id};
                int iScore = p.PlayerDeities.GetDeityById(deityid).Level;
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
                {
                    p.LogPlayerData(PlayerLogTypes.SpecialistsBurnt, setresource - GameConstants.MAX_RESOURCE_STORAGE);
                    setresource = GameConstants.MAX_RESOURCE_STORAGE;
                }
                
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
        public void AddSpecialistIfZero(GameEventSendData ge)
        {
           foreach(var p in _gameContext.PlayerManager.Players)
            {
                foreach(var res in p.PlayerResources.Resources)
                {
                    if(res.Amount == 0)
                    {
                        _gameContext.PlayerManager.ChangeResourceAmount(p, res.Id, 1);
                    }
                }
            }
        }
        public void AddCapitalExpand(GameEventSendData ge)
        {
           foreach(var p in _gameContext.PlayerManager.Players)
            {
                var tile = _gameContext.BoardManager.GetCapitalCity(p.Id);
                if(tile != null)
                {
                    _gameContext.BoardManager.CityExpands(p, tile.dbData.Id);
                }
            }
        }
        public void RemoveCapitalExpand(GameEventSendData ge)
        {
           foreach(var p in _gameContext.PlayerManager.Players)
            {
                var tile = _gameContext.BoardManager.GetCapitalCity(p.Id);
                if(tile != null)
                {
                    _gameContext.BoardManager.CityLoosesExpansion(p, tile.dbData.Id);
                }
            }
        }
        public void LooseHalfWarfareScore(GameEventSendData ge)
        {
           foreach(var p in _gameContext.PlayerManager.Players)
            {
                int iReduce = p.WarfareScore / 2;
                if(iReduce > 0)
                    _gameContext.PlayerManager.ChangeWarfareScore(p, -iReduce);
            }
        }
        public void ScoreForCityStack(GameEventSendData ge)
        {
            ge.gameEventPlayerTable = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.SimpleScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var lrow = new PlayerTableRow(){Player = p.Id};
                int iScore = _gameContext.BoardManager.GetCityStackAmount(p.Id);
                lrow.Value1 = iScore;
                ge.gameEventPlayerTable.ListPlayerRows.Add(lrow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScore, ScorePointType.ErasAndEvents);
            }          
        }


        public void ScoreForRulers(GameEventSendData ge)
        {
            ge.gameEventPlayerTable = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.SimpleScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var lrow = new PlayerTableRow(){Player = p.Id};
                int iScore = p.Rulers.Count;
                lrow.Value1 = iScore;
                ge.gameEventPlayerTable.ListPlayerRows.Add(lrow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScore, ScorePointType.ErasAndEvents);
            }          
        }

        public void ScoreForThreeAngles(GameEventSendData ge, int iAngle1, int iAngle2, int iAngle3)
        {
            ge.gameEventPlayerTable = new GameEventPlayerTable(){PlayerTableType = PlayerTableType.SimpleScore};
            foreach(var p in _gameContext.PlayerManager.GetPlayersInOrder())
            {
                var lrow = new PlayerTableRow(){Player = p.Id};
                int iAngles = 0;
                int iScore = 0;

                if(p.PlayerAngleBoard.HasAngleByValueChecked(iAngle1))
                    iAngles++;
                    
                if(p.PlayerAngleBoard.HasAngleByValueChecked(iAngle2))
                    iAngles++;

                if(p.PlayerAngleBoard.HasAngleByValueChecked(iAngle3))
                    iAngles++;

                if(iAngles == 1)
                    iScore = 1;
                else if(iAngles == 2)
                    iScore = 3;
                else if(iAngles == 3)
                    iScore = 6;
                lrow.Value1 = iScore;
                ge.gameEventPlayerTable.ListPlayerRows.Add(lrow);
                _gameContext.PlayerManager.ChangeScorePoints(p, iScore, ScorePointType.ErasAndEvents);
            }          
        }

        public void DoQueueExtraPhaseAction(GameEventSendData ge)
        {
            int pInOrder = _gameContext.PlayerManager.Players.Count - 1;
            foreach(var player in _gameContext.PlayerManager.GetPlayersInReverseOrder())
            {
                _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.SpecialPlayerAction, ActivePlayers = new List<Guid>(), Value1 = ge.IntValue1, Value2 = pInOrder});
                pInOrder--;
            }
        }
    }
}