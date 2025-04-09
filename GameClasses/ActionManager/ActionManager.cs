using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class ActionManager
    {
        public bool RulerDataDirty = false;
        public bool PhaseStarted = false;
        private ActionLogReturn ActionLogReturn = new ActionLogReturn();
        private List<PlayerBasicSetData> playerActionsChange = new List<PlayerBasicSetData>();
        private List<GameEventSendData> gameEvents = new List<GameEventSendData>();
        private List<ActionTypes> JokerEnabledActions = new List<ActionTypes>(){
            ActionTypes.WAR_TRIBUTE,
        //    ActionTypes.WAR_CONQUEST,
            ActionTypes.WAR_STAR,
            ActionTypes.FOUND_CITY,
            ActionTypes.SKY_OBSERVATION,
            ActionTypes.SKY_RULER_STAR,
            ActionTypes.ERECT_STELAE,
            ActionTypes.CITY_EXPAND_CARVERS,
            ActionTypes.PILGRIMAGE,
            ActionTypes.CITY_EXPAND,
            ActionTypes.MIRRORED_ANGLE
        };
        private List<ActionTypes> CityCardNeutral = new List<ActionTypes>(){
            ActionTypes.FOUND_CITY,
            ActionTypes.PILGRIMAGE
        };
        private List<ActionTypes> CityCardOwned = new List<ActionTypes>(){
            ActionTypes.SKY_OBSERVATION,
            ActionTypes.SKY_RULER_STAR,
            ActionTypes.ERECT_STELAE,
            ActionTypes.PILGRIMAGE,
            ActionTypes.CITY_EXPAND,
            ActionTypes.CITY_EXPAND_CARVERS
        };
        private List<ActionTypes> CityCardEnemy = new List<ActionTypes>(){
            ActionTypes.WAR_TRIBUTE,
        //    ActionTypes.WAR_CONQUEST,
            ActionTypes.WAR_STAR,
            ActionTypes.ERA_MERCENARIES,
            ActionTypes.PILGRIMAGE
        };
        private List<ActionTypes> ActionCardStoneCarving = new List<ActionTypes>(){
            ActionTypes.ERECT_STELAE,
            ActionTypes.RECRUIT_STONE_CARVERS,
            ActionTypes.CITY_EXPAND_CARVERS
        };
        private List<ActionTypes> ActionCardAstronomy = new List<ActionTypes>(){
            ActionTypes.SKY_OBSERVATION,
            ActionTypes.SKY_RULER_STAR,
            ActionTypes.MIRRORED_ANGLE,
            ActionTypes.RECRUIT_ASTRONOMERS
        };
        private List<ActionTypes> ActionCardBuilding = new List<ActionTypes>(){
            ActionTypes.FOUND_CITY,
            ActionTypes.CITY_EXPAND,
            ActionTypes.RECRUIT_BUILDERS
        };
        private List<ActionTypes> ActionCardPilgrimage = new List<ActionTypes>(){
            ActionTypes.PILGRIMAGE,
            ActionTypes.RECRUIT_PRIESTS
        };
        private List<ActionTypes> ActionCardWar = new List<ActionTypes>(){
            ActionTypes.WAR_TRIBUTE,
        //    ActionTypes.WAR_CONQUEST,
            ActionTypes.WAR_STAR,
            ActionTypes.RECRUIT_WARRIORS
        };
        private readonly GameContext _gameContext;
        public ActionChecksManager ActionChecksManager;
        public ActionManager(GameContext gameContext)
        {
            _gameContext = gameContext;
            ActionChecksManager = new ActionChecksManager(gameContext);
        }

        public void AddPlayerBasicSetData(PlayerBasicSetData pbsd)
        {
            playerActionsChange.Add(pbsd);
        }

        public void AddNewGameEvent(GameEventSendData ge)
        {
            gameEvents.Add(ge);
        }

        public List<PlayerBasicSetData> GetListOfPlayersSetData()
        {
            return playerActionsChange;
        }

        public void ActionInitialized()
        {
            playerActionsChange.Clear();
            gameEvents.Clear();
            ActionLogReturn.Active = false;
            RulerDataDirty = false;
            PhaseStarted = false;
        }

        public void BroadCastEndGame()
        {
            var ScorePoints = _gameContext.ScorePointsManager.FinalScore();
            _gameContext.TimerManager.EndTimer();
            var gameTimeSpan = _gameContext.TimerManager.GetTotalGameTime();
            var playerTimeSpan = _gameContext.TimerManager.GetPlayerTimes();

            EndOfGame data = new EndOfGame { PlayerScores = ScorePoints, GameTimeSpan = gameTimeSpan, PlayerTimeSpan = playerTimeSpan,
                    PlayerDataChanges = GetListOfPlayersSetData(),
                    PhaseData = _gameContext.PhaseManager.GetPhaseData(),
                    ActionLog = ActionLogReturn,
                    GameId = _gameContext.GameId,
                    GameEvents = gameEvents};

            _gameContext.EventManager.Broadcast("EndOfGame", ref data);
        }

        public void BroadCastSimlpleChanges()
        {
            if(PhaseStarted && _gameContext.PhaseManager.GetCurrentPhase().PhaseType == PhaseType.EndGame)
            {
                BroadCastEndGame();
                return;
            }

            if(RulerDataDirty)
            {
                FullRulerData fff = new FullRulerData(){
                DeckAmount = _gameContext.RulerCardsManager.GetRulersDeckAmount(), 
                RulerPool = _gameContext.RulerCardsManager.GetRulerPool()};
                EraStartSendData data = new EraStartSendData
                {
                    ActionLog = ActionLogReturn,
                    PlayerDataChanges = GetListOfPlayersSetData(),
                    PhaseData = _gameContext.PhaseManager.GetPhaseData(),
                    FullRulerData = fff,
                    GameEvents = gameEvents
                };
                _gameContext.EventManager.Broadcast("EraStartChanges", ref data);
            }
            else if(PhaseStarted)
            {
                PhaseSendData data = new PhaseSendData
                {
                    ActionLog = ActionLogReturn,
                    PlayerDataChanges = GetListOfPlayersSetData(),
                    PhaseData = _gameContext.PhaseManager.GetPhaseData(),
                    GameEvents = gameEvents
                };
                _gameContext.EventManager.Broadcast("PhaseStarted", ref data);
            }
            else // no specific markers, just simple data
            {
                SimpleSendData data = new SimpleSendData
                {
                    ActionLog = ActionLogReturn,
                    PlayerDataChanges = GetListOfPlayersSetData()
                };
                _gameContext.EventManager.Broadcast("SimpleChanges", ref data);
            }
            ActionInitialized();
            _gameContext.CreateBackupLobbyGame();
        }
        public bool HasAdjacentCity(Tile tile, Guid playerId)
        {
            bool bAdjacentCityFound = false;
            foreach(var pTile in _gameContext.BoardManager.GetTilesInRange(tile, 1))
            {
                if(pTile.gameData.OwnerId == playerId)
                    return true;
            }
            return false;
        }

        public bool ChooseActionCardsToHand(List<int> cards, PlayerInGame player)
        {
            if(cards != null && cards.Count() == 5)
            {
                if(cards.Distinct().Count() == cards.Count())
                {
                    foreach(int index in cards)
                    {
                        if(player.ReserveActionCards.FirstOrDefault(p => p.GameIndex == index) == null)
                            return false;
                    }
                    foreach(int index in cards)
                    {
                        var card = player.ReserveActionCards.FirstOrDefault(p => p.GameIndex == index);
                        player.ReserveActionCards.Remove(card);
                        player.HandActionCards.Add(card);
                    }
                    _gameContext.ActionCardManager.SendPlayerActionCardData(player);
                    _gameContext.PhaseManager.PlayerFinishedCurrentPhase(player);
                    return true;
                }
            }
            return false;
        }

        public bool ReceivedActionForm(ActionFormSend af, PlayerInGame player)
        {
            if(!CanProceedActionForm(af, player))
            {
                Console.WriteLine("Akcja nie może być wykonana.");
                return false;
            }

            DoProceedActionForm(af, player);
            return true;
        }
        
        public int GetResourceCostOfAction(PlayerInGame player, ActionGameData dbinfo)
        {
            int cost = dbinfo.StaticCost;
            if(dbinfo.PerEraCost != 0)
                cost += dbinfo.PerEraCost * _gameContext.PhaseManager.CurrentEra;

            if(dbinfo.PerRulerCost != 0)
                cost += player.Rulers.Count;

            if(dbinfo.PerCityLevelCost != 0)
            {
                var capital = _gameContext.BoardManager.GetCapitalCity(player.Id);
                if(capital != null)
                    cost += dbinfo.PerCityLevelCost * capital.gameData.Level;
            }

            if(dbinfo.PerCityCost100 != 0)
                cost += (dbinfo.PerCityCost100 * _gameContext.BoardManager.GetNumCities(player.Id)) / 100;

            if(dbinfo.MaxCost != -1 && dbinfo.MaxCost < cost)
                return dbinfo.MaxCost;

            return cost;
        }

        public void MakePlayerPayForActionId(PlayerInGame player, int actionid)
        {
            var action = GameDataManager.GetActionById(actionid);
            if(action.Resource == -1)
                return;
            
            int iCost = GetResourceCostOfAction(player, action);
            if(iCost > 0)
            {
                player.LogPlayerData(PlayerLogTypes.SpecialistsUsed, iCost);
                _gameContext.PlayerManager.ChangeResourceAmount(player, action.Resource, -1 * iCost);
            }
        }
        public bool CanPlayerPayForAction(PlayerInGame player, ActionGameData dbinfo, int jokerActionId = -1)
        {
            if(dbinfo.Resource == -1)
                return true;

            int iTotalCost = GetResourceCostOfAction(player, dbinfo);
            if(jokerActionId != -1)
            {
                var jokeraction = GameDataManager.GetActionById(jokerActionId);
                if(jokeraction.Resource == dbinfo.Resource)
                    iTotalCost += GetResourceCostOfAction(player, jokeraction);
            }
            return player.PlayerResources.GetResourceById(dbinfo.Resource).Amount >= iTotalCost;
        }

        public bool CanPlayerTargetTileForAngle(PlayerInGame player, int tileid)
        {
            var tile = _gameContext.BoardManager.GetTileById(tileid);
            if(tile == null || tile.Jungle)
                return false;

            if(tile.gameData.OwnerId == player.Id)
                return true;

            if(tile.dbData.DeityId != -1)
            {
                if(player.PlayerDeities.GetDeityById(tile.dbData.DeityId).Level >= 2)
                    return true;
            }

            if(tile.dbData.TileTypeId == 4 && IsCurrentEraId(13))
                return true;

            if(IsCurrentEraId(14) && tile.gameData.OwnerId != Guid.Empty && tile.gameData.Level > 0)
                return true;

            return false;
        }

        public bool CanPlayerMakeAngleBetweenTiles(PlayerInGame player, int fromtileid, int totileid)
        {   
            if(!CanPlayerTargetTileForAngle(player, totileid))
                return false;

            if(IsCurrentEraId(21) && !_gameContext.BoardManager.IsInRange(fromtileid, totileid, 1))
                return false;

            int iAngle = _gameContext.BoardManager.GetPlayerBoardAngleBetween(player, fromtileid, totileid);
            if(iAngle == -1)
                return false;

            if(player.PlayerAngleBoard.GetAngleById(iAngle).bChecked)
                return false;

            return true;
        }

        public bool IsTileValidForDeity(PlayerInGame player, int tileid, int deityid)
        {
            var tile = _gameContext.BoardManager.GetTileById(tileid);
            if(tile == null)
                return false;

            if(tile.dbData.AdjExtraDeityId == deityid)
                return true;
            
            foreach(var pTile in _gameContext.BoardManager.GetTilesInRange(tile, 1))
            {
                if(pTile.dbData.DeityId == deityid)
                    return true;
                else if(pTile.dbData.TileTypeId == 1 && player.PlayerDeities.GetDeityById(deityid).Level >= 2)
                    return true;
            }
            Console.WriteLine("Bóstwo nie jest valid! {0}, {1}", tileid, deityid);
            return false;
        }

        public bool CanProceedActionForm(ActionFormSend af, PlayerInGame player)
        {
            if(af.ActionId == -1 || (af.CardId == -1 && af.EventCardId == -1))
                return false;

            ActionCard? actionCard;
            int actionCardId = -1;
            int actionCardLocationId = -1;
            bool bEventCard = false;
            if(_gameContext.PhaseManager.GetCurrentPhase().Value1 != -1)
            {
                EventGameData egd = GameDataManager.GetEventById(_gameContext.PhaseManager.GetCurrentPhase().Value1);
                if(af.CardId != -1)
                    return false;

                if(af.Joker)
                    return false;

                if(af.EventCardId != egd.Id)
                    return false;

                if(af.PassOnAction)
                    return egd.EffectVal1 == 1;

                if(af.ActionId != egd.EffectVal1)
                    return false;

                bEventCard = true;
            }
            else if(af.EventCardId != -1)
                return false;
            else
            {
                actionCard = player.HandActionCards.FirstOrDefault(card => card.GameIndex == af.CardId);
                if(actionCard == null)
                    return false;

                actionCardId = actionCard.Id;
                actionCardLocationId = actionCard.LocationId;

                if(player.HandActionCards.Count == 1 && player.IncomingOrder == -1 && af.ActionId != (int) ActionTypes.DISCARD_CARD)
                {
                    if(!IsCurrentEraId(17) || af.ActionId != (int) ActionTypes.WAR_TRIBUTE)
                        return false;
                }
            }
            


            if(af.ActionId == (int) ActionTypes.DISCARD_CARD)
            {
                if(af.Resource1Id == -1)
                    return false;

                if(IsCurrentEraId(22) && (af.Resource2Id == -1))
                    return false;

            //    if(af.Resource2Id == -1 && player.IncomingOrder == -1)
            //        return false;

                return true;
            }

            // check if card <> action is properly connected
            if(af.Joker)
            {
                var jokeraction = GameDataManager.GetActionById(af.JokerActionId);
                if(jokeraction.RequiresEffectId != -1)
                {
                    if(!_gameContext.PlayerManager.HasPlayerAuraEffectId(player, jokeraction.RequiresEffectId))
                        return false;
                }

                if(!jokeraction.Joker)
                    return false;

                if(!CanPlayerPayForAction(player, jokeraction))
                    return false;

                if(!JokerEnabledActions.Contains((ActionTypes) af.ActionId))
                    return false;
            }
            else
            {
                if(actionCardId == 1 && actionCardLocationId != -1) // city card
                {
                    var tile = _gameContext.BoardManager.GetTileById(actionCardLocationId);
                    if(tile.Jungle)
                    {
                        Console.WriteLine("Pole jest dżunglą");
                        return false;
                    }

                    if(actionCardLocationId != af.TileId)
                        return false;
                    
                    if(tile.dbData.TileTypeId != 3)
                    {
                        Console.WriteLine("Nieprawidłowy typ pola!");
                        return false;
                    }
                    else
                    {
                        if(tile.gameData.OwnerId == Guid.Empty)
                        {
                            if(!CityCardNeutral.Contains((ActionTypes) af.ActionId))
                                return false;
                        }
                        else if(tile.gameData.OwnerId == player.Id)
                        {
                            if(!CityCardOwned.Contains((ActionTypes) af.ActionId))
                                return false;
                        }
                        else
                        {
                            if(!CityCardEnemy.Contains((ActionTypes) af.ActionId))
                                return false;
                        }
                    }
                }
                else if(actionCardId == 2)
                {
                    if(!ActionCardStoneCarving.Contains((ActionTypes) af.ActionId))
                        return false;
                }
                else if(actionCardId == 3)
                {
                    if(!ActionCardAstronomy.Contains((ActionTypes) af.ActionId))
                        return false;
                }
                else if(actionCardId == 4)
                {
                    if(!ActionCardBuilding.Contains((ActionTypes) af.ActionId))
                        return false;
                }
                else if(actionCardId == 5)
                {
                    if(!ActionCardPilgrimage.Contains((ActionTypes) af.ActionId))
                        return false;
                }
                else if(actionCardId == 6)
                {
                    if(!ActionCardWar.Contains((ActionTypes) af.ActionId))
                        return false;
                }
            }
            var actionInfo = GameDataManager.GetActionById(af.ActionId);
            if(actionInfo.SpecialExtra)
            {
                if(!_gameContext.GameOptions.SpecialActions)
                {
                    if(af.EventCardId == -1)
                        return false;
                }
            }


            if(!CanPlayerPayForAction(player, actionInfo, af.JokerActionId))
                return false;

            if(actionInfo.RequiresEffectId != -1 && !bEventCard)
            {
                if(!_gameContext.PlayerManager.HasPlayerAuraEffectId(player, actionInfo.RequiresEffectId))
                    return false;
            }

            // check location id
            if(actionInfo.RequiresLocation)
            {
                if(af.TileId == -1)
                    return false;

                var tile = _gameContext.BoardManager.GetTileById(af.TileId);
                if(tile.Jungle)
                {
                    Console.WriteLine("Błąd. Pole jest dżunglą.");
                    return false;
                }
                
                if(tile.dbData.TileTypeId != 3)
                {
                    Console.WriteLine("Nieprawidłowy typ pola!");
                    return false;
                }

                if(af.ActionId == (int) ActionTypes.ERECT_STELAE)
                {
                    if(tile.gameData.OwnerId != player.Id)
                        return false;

                 //   if(tile.gameData.RulerStelae != -1)
                 //       return false;
                }
                else if(af.ActionId == (int) ActionTypes.FOUND_CITY)
                {
                    if(tile.gameData.OwnerId != Guid.Empty)
                        return false;

                    if(actionCardId == 4 || af.Joker || bEventCard)
                    {
                        if(!HasAdjacentCity(tile, player.Id))
                            return false;
                    }
                }
                else if(af.ActionId == (int) ActionTypes.PILGRIMAGE)
                {
                    if(tile.gameData.OwnerId != player.Id)
                    {
                        if(actionCardId == 5 || af.Joker || bEventCard)
                        {
                            if(!HasAdjacentCity(tile, player.Id))
                                return false;
                        }
                    }
                }
                else if(af.ActionId == (int) ActionTypes.PILGRIMAGE_RIVAL)
                {
                    if(tile.gameData.OwnerId == player.Id || tile.gameData.OwnerId == Guid.Empty)
                        return false;

                    if(actionCardId == 5 || af.Joker || bEventCard)
                    {
                        if(!HasAdjacentCity(tile, player.Id))
                            return false;
                    }
                }
                else if(af.ActionId == (int) ActionTypes.CITY_EXPAND)
                {
                    if(tile.gameData.OwnerId != player.Id)
                        return false;
                    
                    if(tile.gameData.Level == 0 && (_gameContext.BoardManager.GetCapitalCity(player.Id) != null))
                        return false;

                    if(!CanExpandCapital(player))
                        return false;
                }
                else if(af.ActionId == (int) ActionTypes.CITY_EXPAND_CARVERS)
                {
                    if(tile.gameData.OwnerId != player.Id)
                        return false;
                    
                    if(tile.gameData.Level == 0 && (_gameContext.BoardManager.GetCapitalCity(player.Id) != null))
                        return false;

                    if(player.Rulers.Count() < 4)
                        return false;

                    if(!CanExpandCapital(player))
                        return false;
                }
                else if(af.ActionId == (int) ActionTypes.SKY_OBSERVATION)
                {
                    if(tile.gameData.OwnerId != player.Id)
                        return false;
                }
                else if(af.ActionId == (int) ActionTypes.LOOSE_CITY)
                {
                    if(tile.gameData.OwnerId != player.Id)
                        return false;
                }
                else if(af.ActionId == (int) ActionTypes.SKY_RULER_STAR)
                {
                    if(tile.gameData.OwnerId != player.Id)
                        return false;

                 //   if(tile.gameData.RulerStelae == -1)
                  //      return false;
                }
                else if(af.ActionId == (int) ActionTypes.WAR_TRIBUTE)
                {
                    if(tile.gameData.OwnerId == player.Id || tile.gameData.OwnerId == Guid.Empty)
                        return false;

                    if((actionCardId == 6 || af.Joker || bEventCard) && !IsCurrentEraId(10))
                    {
                        if(!HasAdjacentCity(tile, player.Id))
                            return false;
                    }

                    if(af.Resource1Id != tile.dbData.Resource1 && af.Resource1Id != tile.dbData.Resource2)
                        return false;
                }
                else if(af.ActionId == (int) ActionTypes.ERA_MERCENARIES)
                {
                    if(tile.gameData.OwnerId == player.Id || tile.gameData.OwnerId == Guid.Empty)
                        return false;

                    if(player.PlayerLuxuries.GetLuxuryById(tile.dbData.LuxuryId).Amount == 0)
                        return false;
                }
                else if(af.ActionId == (int) ActionTypes.WAR_CONQUEST)
                {
                    var seconplayerid = tile.gameData.OwnerId;
                    if(seconplayerid == player.Id || seconplayerid == Guid.Empty)
                        return false;

                    if((actionCardId == 6 || af.Joker || bEventCard) && !IsCurrentEraId(10))
                    {
                        if(!HasAdjacentCity(tile, player.Id))
                            return false;
                    }

                    if(tile.gameData.Level > 0)
                        return false;

                    int iTargetNumCities = _gameContext.BoardManager.GetNumCities(seconplayerid);
                    if(iTargetNumCities < 6)
                    {
                        if(iTargetNumCities <= _gameContext.BoardManager.GetNumCities(player.Id))
                            return false;
                    }
                }
                else if(af.ActionId == (int) ActionTypes.WAR_STAR)
                {
                    var seconplayerid = tile.gameData.OwnerId;
                    if(seconplayerid == player.Id || seconplayerid == Guid.Empty)
                        return false;

                    if((actionCardId == 6 || af.Joker || bEventCard) && !IsCurrentEraId(10))
                    {
                        if(!HasAdjacentCity(tile, player.Id))
                            return false;
                    }

                //    if(tile.gameData.Level == 0)
                //        return false;
                }
            }
            
            // check specific actions
            if(af.ActionId == (int) ActionTypes.PILGRIMAGE)
            {
                if(af.DeityId == -1)
                    return false;

                if(!IsTileValidForDeity(player, af.TileId, af.DeityId))
                    return false;
            }
            else if(af.ActionId == (int) ActionTypes.PILGRIMAGE_RIVAL)
            {
                if(af.DeityId == -1)
                    return false;

                if(!IsTileValidForDeity(player, af.TileId, af.DeityId))
                    return false;
            }
            else if(af.ActionId == (int) ActionTypes.SWAP_DEITIES_LEVELS)
            {
                if(af.DeityId == -1 || af.ExtraInfoId == -1 || af.DeityId == af.ExtraInfoId)
                    return false;

                if(player.PlayerDeities.GetDeityById(af.DeityId).Level == player.PlayerDeities.GetDeityById(af.ExtraInfoId).Level)
                    return false;
            }
            else if(af.ActionId == (int) ActionTypes.WAR_STAR)
            {
                if(af.ExtraInfoTypeId == -1)
                    return false;
                else if(af.ExtraInfoTypeId == 1)
                {
                    if(af.ExtraInfoId == -1)
                        return false;

                    Guid targetplayerId = _gameContext.BoardManager.GetTileById(af.TileId).gameData.OwnerId;
                    PlayerInGame targetplayer = _gameContext.PlayerManager.GetPlayerById(targetplayerId);
                    if(targetplayer.PlayerDeities.GetDeityById(af.ExtraInfoId).Level <= player.PlayerDeities.GetDeityById(af.ExtraInfoId).Level)
                        return false;
                }
                else if(af.ExtraInfoTypeId == 2)
                {
                    if(af.ExtraInfoId == -1)
                        return false;

                    Guid targetplayerId = _gameContext.BoardManager.GetTileById(af.TileId).gameData.OwnerId;
                    PlayerInGame targetplayer = _gameContext.PlayerManager.GetPlayerById(targetplayerId);
                    if(!targetplayer.PlayerAngleBoard.GetAngleById(af.ExtraInfoId).bChecked || player.PlayerAngleBoard.GetAngleById(af.ExtraInfoId).bChecked)
                        return false;
                }
                else if(af.ExtraInfoTypeId == 3)
                {
                    // always fine
                }
                else if(af.ExtraInfoTypeId == 4)
                {
                    Tile cTile = _gameContext.BoardManager.GetTileById(af.TileId);
                    Guid targetplayerId = cTile.gameData.OwnerId;

                    if(cTile.gameData.Level > 0)
                        return false;
                    
                    if(!CanConquerEnemyCity(targetplayerId, player.Id))
                        return false;
                }
                else
                    return false; // TUTAJ CHECK CONQUEST
            }
            else if(af.ActionId == (int) ActionTypes.ERECT_STELAE)
            {
                if(af.RulerCardId == 0)
                {
                    if(_gameContext.RulerCardsManager.GetRulersDeckAmount() == 0)
                        return false;
                }
                else if(!_gameContext.RulerCardsManager.HasPoolRulerId(af.RulerCardId))
                    return false;
            }
            else if(af.ActionId == (int) ActionTypes.SKY_OBSERVATION)
            {
                if(af.TileSecondId == -1)
                    return false;

                if(!CanPlayerMakeAngleBetweenTiles(player, af.TileId, af.TileSecondId))
                    return false;
            }
            else if(af.ActionId == (int) ActionTypes.SKY_RULER_STAR)
            {
                int angleid = af.ExtraInfoId;
                if(!_gameContext.PlayerManager.HasPlayerRulerWithAngleId(player, angleid))
                    return false;

                if(player.PlayerAngleBoard.GetAngleById(angleid).bChecked)
                    return false;
            }
            else if(af.ActionId == (int) ActionTypes.MIRRORED_ANGLE)
            {
                int angleid = af.ExtraInfoId;
                if(player.PlayerAngleBoard.GetAngleById(angleid).bChecked)
                    return false;

                if(!player.PlayerAngleBoard.GetMirroredAngleById(angleid).bChecked)
                    return false;
            }
            else if(af.ActionId == (int) ActionTypes.SPECIALISTS_INTO_POINTS)
            {
                if(af.ExtraInfoId < 0 || af.ExtraInfoId > 6)
                    return false;

                int specId = GameDataManager.GetEventById(af.EventCardId).EffectVal3;
                if(player.PlayerResources.GetResourceById(specId).Amount < af.ExtraInfoId)
                    return false;
            }
            return true;
        }

        public void DoProceedActionForm(ActionFormSend af, PlayerInGame player)
        {
            if(af.PassOnAction)
            {
                ConvertFormIntoActionLog(player, af);
                _gameContext.PhaseManager.PlayerFinishedCurrentPhase(player);
                return;
            }

            player.LogAction((ActionTypes) af.ActionId);

            if(af.ActionId == (int) ActionTypes.DISCARD_CARD)
            {
                _gameContext.PlayerManager.ChangeResourceAmount(player, af.Resource1Id, 1);
                if(player.IncomingOrder == -1)
                    _gameContext.PlayerManager.ClaimOrderForNextTurn(player);

                if(IsCurrentEraId(22))
                    _gameContext.PlayerManager.ChangeResourceAmount(player, af.Resource2Id, 1);

            }
            if(af.Joker)
            {
                player.LogPlayerData(PlayerLogTypes.JokerActions);
                MakePlayerPayForActionId(player, af.JokerActionId);
                if(IsCurrentEraId(23))
                    _gameContext.PlayerManager.ChangeScorePoints(player, 1, ScorePointType.ErasAndEvents);
            }

            MakePlayerPayForActionId(player, af.ActionId);

            if(af.ActionId == (int) ActionTypes.ERECT_STELAE)
            {
                _gameContext.RulerCardsManager.PlayerAcquiredRulerCard(player, af.RulerCardId, af.TileId);
            }
            else if(af.ActionId == (int) ActionTypes.FOUND_CITY)
            {
                _gameContext.BoardManager.CityFoundOrConquest(player, af.TileId);
            }
            else if(af.ActionId == (int) ActionTypes.CITY_EXPAND)
            {
                _gameContext.BoardManager.CityExpands(player, af.TileId);
            }
            else if(af.ActionId == (int) ActionTypes.CITY_EXPAND_CARVERS)
            {
                _gameContext.BoardManager.CityExpands(player, af.TileId);
            }
            else if(af.ActionId == (int) ActionTypes.PILGRIMAGE)
            {
                if(IsCurrentEraId(18) && player.PlayerDeities.GetDeityById(af.DeityId).Level == 0)
                    _gameContext.PlayerManager.IncreaseDeityLevel(player, af.DeityId);

                _gameContext.PlayerManager.IncreaseDeityLevel(player, af.DeityId);
            }
            else if(af.ActionId == (int) ActionTypes.SWAP_DEITIES_LEVELS)
            {
                _gameContext.PlayerManager.SwapDeityLevels(player, af.DeityId, af.ExtraInfoId);
            }
            else if(af.ActionId == (int) ActionTypes.PILGRIMAGE_RIVAL)
            {
                if(IsCurrentEraId(18) && player.PlayerDeities.GetDeityById(af.DeityId).Level == 0)
                    _gameContext.PlayerManager.IncreaseDeityLevel(player, af.DeityId);
                
                if(IsCurrentEraId(15))
                    _gameContext.PlayerManager.ChangeWarfareScore(player, 1);

                _gameContext.PlayerManager.IncreaseDeityLevel(player, af.DeityId);
            }
            else if(af.ActionId == (int) ActionTypes.RECRUIT_ASTRONOMERS)
            {
                _gameContext.PlayerManager.ChangeResourceAmount(player, 3, 2);
            }
            else if(af.ActionId == (int) ActionTypes.RECRUIT_BUILDERS)
            {
                _gameContext.PlayerManager.ChangeResourceAmount(player, 2, 2);
            }
            else if(af.ActionId == (int) ActionTypes.RECRUIT_PRIESTS)
            {
                _gameContext.PlayerManager.ChangeResourceAmount(player, 5, 2);
            }
            else if(af.ActionId == (int) ActionTypes.RECRUIT_STONE_CARVERS)
            {
                _gameContext.PlayerManager.ChangeResourceAmount(player, 4, 2);
            }
            else if(af.ActionId == (int) ActionTypes.RECRUIT_WARRIORS)
            {
                _gameContext.PlayerManager.ChangeResourceAmount(player, 1, 2);
            }
            else if(af.ActionId == (int) ActionTypes.SKY_OBSERVATION)
            {
                _gameContext.PlayerManager.CheckAngle(player, _gameContext.BoardManager.GetPlayerBoardAngleBetween(player, af.TileId, af.TileSecondId));
            }
            else if(af.ActionId == (int) ActionTypes.MIRRORED_ANGLE)
            {
                _gameContext.PlayerManager.CheckAngle(player, af.ExtraInfoId);
            }
            else if(af.ActionId == (int) ActionTypes.SKY_RULER_STAR)
            {
                _gameContext.PlayerManager.CheckAngle(player, af.ExtraInfoId);
            }
            else if(af.ActionId == (int) ActionTypes.WAR_TRIBUTE)
            {
                var tile = _gameContext.BoardManager.GetTileById(af.TileId);
                if(af.Resource1Id == tile.dbData.Resource1)
                {
                    _gameContext.PlayerManager.ChangeResourceAmount(player, tile.dbData.Resource1, 2);
                    _gameContext.PlayerManager.ChangeResourceAmount(player, tile.dbData.Resource2, 1);
                }
                else
                {
                    _gameContext.PlayerManager.ChangeResourceAmount(player, tile.dbData.Resource1, 1);
                    _gameContext.PlayerManager.ChangeResourceAmount(player, tile.dbData.Resource2, 2);
                }
            //    _gameContext.PlayerManager.ChangeWarfareScore(player, GetWarfareScoreFromTribute());
                if(IsCurrentEraId(17) && player.IncomingOrder == -1)
                    _gameContext.PlayerManager.ClaimOrderForNextTurn(player);
            }
            else if(af.ActionId == (int) ActionTypes.WAR_CONQUEST)
            {
                Tile tile = _gameContext.BoardManager.GetTileById(af.TileId);
                PlayerInGame ConqueredPlayed = _gameContext.PlayerManager.GetPlayerById(tile.gameData.OwnerId);
                _gameContext.PlayerManager.ChangeResourceAmount(ConqueredPlayed, tile.dbData.Resource1, 1);
                _gameContext.PlayerManager.ChangeResourceAmount(ConqueredPlayed, tile.dbData.Resource2, 1);
                if(_gameContext.PlayerManager.HasNeedOfExtraConverting(ConqueredPlayed))
                    _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.IncomeConverting, ActivePlayers = new List<Guid>(){ConqueredPlayed.Id}});
                
                _gameContext.BoardManager.CityFoundOrConquest(player, af.TileId);
                _gameContext.PlayerManager.ChangeWarfareScore(player, GetWarfareScoreFromConquest());
            }
            else if(af.ActionId == (int) ActionTypes.WAR_STAR)
            {
                if(af.ExtraInfoTypeId == 1)
                {
                    _gameContext.PlayerManager.IncreaseDeityLevel(player, af.ExtraInfoId);
                //    _gameContext.PlayerManager.ChangeWarfareScore(player, GetWarfareScoreFromStarWar());
                }
                else if(af.ExtraInfoTypeId == 2)
                {
                    _gameContext.PlayerManager.CheckAngle(player, af.ExtraInfoId);
               //     _gameContext.PlayerManager.ChangeWarfareScore(player, GetWarfareScoreFromStarWar());
                }
                else if(af.ExtraInfoTypeId == 3)
                {
                    _gameContext.PlayerManager.ChangeWarfareScore(player, 2 + GetWarfareScoreFromStarWar());
                }
                else if(af.ExtraInfoTypeId == 4)
                {
                    Tile tile = _gameContext.BoardManager.GetTileById(af.TileId);
                    PlayerInGame ConqueredPlayed = _gameContext.PlayerManager.GetPlayerById(tile.gameData.OwnerId);
                    _gameContext.PlayerManager.ChangeResourceAmount(ConqueredPlayed, 1, 2);
                //    _gameContext.PlayerManager.ChangeResourceAmount(ConqueredPlayed, tile.dbData.Resource1, 1);
                //    _gameContext.PlayerManager.ChangeResourceAmount(ConqueredPlayed, tile.dbData.Resource2, 1);
                    if(_gameContext.PlayerManager.HasNeedOfExtraConverting(ConqueredPlayed))
                        _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.IncomeConverting, ActivePlayers = new List<Guid>(){ConqueredPlayed.Id}});
                
                    _gameContext.BoardManager.CityFoundOrConquest(player, af.TileId);
                }
            }
            else if(af.ActionId == (int) ActionTypes.LOOSE_CITY)
            {
                _gameContext.BoardManager.CityLost(player, af.TileId);
            }
            else if(af.ActionId == (int) ActionTypes.SPECIALISTS_INTO_POINTS)
            {
                if(af.ExtraInfoId > 0)
                {
                    player.LogPlayerData(PlayerLogTypes.SpecialistsUsed, af.ExtraInfoId);
                    _gameContext.PlayerManager.ChangeResourceAmount(player, GameDataManager.GetEventById(af.EventCardId).EffectVal3, -1 * af.ExtraInfoId);
                    _gameContext.PlayerManager.ChangeScorePoints(player, af.ExtraInfoId, ScorePointType.ErasAndEvents);
                }
            }
            else if(af.ActionId == (int) ActionTypes.ERA_MERCENARIES)
            {
                var tile = _gameContext.BoardManager.GetTileById(af.TileId);
                _gameContext.PlayerManager.ChangeResourceAmount(player, tile.dbData.Resource1, 2);
                _gameContext.PlayerManager.ChangeResourceAmount(player, tile.dbData.Resource2, 2);
            }
            
            ConvertFormIntoActionLog(player, af);
    
            if(af.CardId != -1)
                _gameContext.PlayerManager.UsedActionCardFromHand(player, af.CardId);
            
            if(_gameContext.PlayerManager.HasNeedOfExtraConverting(player))
                _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.IncomeConverting, ActivePlayers = new List<Guid>(){player.Id}});

            _gameContext.PhaseManager.PlayerFinishedCurrentPhase(player);
        }

        public void ConvertFormIntoActionLog(PlayerInGame player, ActionFormSend af)
        {
            ActionLogReturn = new ActionLogReturn();
            ActionLogReturn.Active = true;
            ActionLogReturn.PlayerId = player.Id;
            ActionLogReturn.JokerActionId = af.JokerActionId;
            ActionLogReturn.ActionId = af.ActionId;
            ActionLogReturn.DeityId = af.DeityId;
            ActionLogReturn.ExtraInfoId = af.ExtraInfoId;
            ActionLogReturn.ExtraInfoTypeId = af.ExtraInfoTypeId;
            ActionLogReturn.EventCardId = af.EventCardId;
            ActionLogReturn.PassOnAction = af.PassOnAction; 
            if(af.ActionId == (int) ActionTypes.DISCARD_CARD)
            {
                ActionLogReturn.Resource1Id = af.Resource1Id;
                ActionLogReturn.Resource2Id = af.Resource2Id;
            }
            else if(!af.Joker && (af.CardId != -1))
            {
                ActionGameData agd = GameDataManager.GetActionById(af.ActionId);
                ActionLogReturn.CardGameId = af.CardId;
                if(agd.ShowCardAll)
                {
                    var card = player.HandActionCards.FirstOrDefault(c => c.GameIndex == af.CardId);
                    if(card != null)
                    {
                        ActionLogReturn.CardId = card.Id;
                        ActionLogReturn.CardLocationId = card.LocationId;
                    }
                }
            }
        }

        public bool ConvertResourceDuringEndGame(int id, PlayerInGame player)
        {
            foreach(var resource in player.PlayerResources.Resources)
            {
                if(resource.Converters.Contains(id))
                {
                    ResourceConverterGameData dbinfo = GameDataManager.GetResourceConverterById(id);
                    if(player.PlayerResources.GetResourceById(dbinfo.FromResource).Amount >= dbinfo.FromValue)
                    {
                        _gameContext.PlayerManager.ApplyResourceConverter(player, dbinfo);
                        if(_gameContext.PlayerManager.HasNeedOfEndGameConverting(player))
                            BroadCastSimlpleChanges();
                        else
                        {
                            _gameContext.PlayerManager.TrimResourceOverStorage(player);
                            _gameContext.PhaseManager.PlayerFinishedCurrentPhase(player);
                        }

                        return true;
                    }
                    break;
                }
            }
            return false;
        }
        public bool ConvertResourceDuringExtraPhase(int id, PlayerInGame player)
        {
            if(IsCurrentEraId(2))
                return false;

            foreach(var resource in player.PlayerResources.Resources)
            {
                if(resource.Converters.Contains(id))
                {
                    ResourceConverterGameData dbinfo = GameDataManager.GetResourceConverterById(id);
                    if(player.PlayerResources.GetResourceById(dbinfo.FromResource).Amount > GameConstants.MAX_RESOURCE_STORAGE)
                    {
                        _gameContext.PlayerManager.ApplyResourceConverter(player, dbinfo);
                        if(_gameContext.PlayerManager.HasNeedOfExtraConverting(player))
                            BroadCastSimlpleChanges();
                        else
                            _gameContext.PhaseManager.PlayerFinishedCurrentPhase(player);

                        return true;
                    }
                    break;
                }
            }
            return false;
        }
        public bool ConvertResourceDuringPlayerAction(int id, PlayerInGame player)
        {
            foreach(var resource in player.PlayerResources.Resources)
            {
                if(resource.Converters.Contains(id))
                {
                    ResourceConverterGameData dbinfo = GameDataManager.GetResourceConverterById(id);
                    if(player.PlayerResources.GetResourceById(dbinfo.FromResource).Amount >= dbinfo.FromValue)
                    {
                        _gameContext.PlayerManager.ApplyResourceConverter(player, dbinfo);
                        BroadCastSimlpleChanges();
                        return true;
                    }
                    break;
                }
            }
            return false;
        }
        public bool PassExtraConvertPhase(PlayerInGame player)
        {
            _gameContext.PlayerManager.TrimResourceOverStorage(player);
            _gameContext.PhaseManager.PlayerFinishedCurrentPhase(player);
            return true;
        }

        public bool PassEndGameConvertPhase(PlayerInGame player)
        {
            _gameContext.PlayerManager.TrimResourceOverStorage(player);
            _gameContext.PhaseManager.PlayerFinishedCurrentPhase(player);
            return true;
        }

        public bool IsCurrentEraId(int iCheck)
        {
            return iCheck == _gameContext.EraEffectManager.CurrentAgeCardId;
        }
        public int GetExtraWarfareScore()
        {
            int iExtra = 0;
            if(IsCurrentEraId(11))
                iExtra++;

            return iExtra;
        }
        public int GetWarfareScoreFromTribute()
        {
            return 1 + GetExtraWarfareScore();
        }
        public int GetWarfareScoreFromConquest()
        {
            return 2 + GetExtraWarfareScore();
        }
        public int GetWarfareScoreFromStarWar()
        {
            return 3 + GetExtraWarfareScore();
        }

        public bool CanConquerEnemyCity(Guid targetplayer, Guid attackingplayer)
        {
            if(targetplayer == Guid.Empty)
                return false;

            if(targetplayer == attackingplayer)
                return false;

            int numcities = _gameContext.BoardManager.GetNumCities(targetplayer);
            foreach(var tsp in _gameContext.PlayerManager.Players)
            {
                if(tsp.Id != attackingplayer)
                {
                    if(_gameContext.BoardManager.GetNumCities(tsp.Id) > numcities)
                        return false;
                }
            }            

            if(numcities >= 5)
                return true;

            return _gameContext.BoardManager.GetNumCities(attackingplayer) <= numcities;
        }

        public bool CanExpandCapital(PlayerInGame player)
        {
            var capitaltile = _gameContext.BoardManager.GetCapitalCity(player.Id);
            int icapitallevel = 0;

            if(capitaltile != null)
                icapitallevel = capitaltile.gameData.Level;

            if((icapitallevel < _gameContext.BoardManager.GetNumCities(player.Id)) || !_gameContext.BoardManager.IsThereAtLeastOneNeutralCityLocation())
                return true;

            return false;
        }
    }
}
