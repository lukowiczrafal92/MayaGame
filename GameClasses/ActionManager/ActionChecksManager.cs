using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class ActionChecksManager
    {
        private readonly GameContext _gameContext;
        public ActionChecksManager(GameContext gameContext)
        {
            _gameContext = gameContext;
        }


        public bool DoPlayerNeedAction(PlayerInGame player, int iEventCard)
        {
            var dbEventInfo = GameDataManager.GetEventById(iEventCard);
            var dbActionInfo = GameDataManager.GetActionById(dbEventInfo.EffectVal1);

        // would require to check all possible converts :)
        //    if(!_gameContext.ActionManager.CanPlayerPayForAction(player, dbActionInfo))
        //           return false;

            // if any available
            if(dbEventInfo.EffectVal1 == (int) ActionTypes.PILGRIMAGE)
                return HasPlayerAtLeastOneCity(player.Id);
            else if(dbEventInfo.EffectVal1 == (int) ActionTypes.FOUND_CITY)
                return HasAnyValidTileCity(player.Id, false, false, true, true);
            else if(dbEventInfo.EffectVal1 == (int) ActionTypes.ERECT_STELAE)
            {
                if(!HasPlayerAtLeastOneCity(player.Id))
                    return false;

                return _gameContext.RulerCardsManager.AnyOptionLeft();
            }
            else if(dbEventInfo.EffectVal1 == (int) ActionTypes.SKY_OBSERVATION)
            {
                if(!HasPlayerAtLeastOneCity(player.Id))
                    return false;

                return CanMakeAnyAngle(player.Id);
            }
            else if(dbEventInfo.EffectVal1 == (int) ActionTypes.WAR_TRIBUTE)
                return HasAnyValidTileCity(player.Id, false, true, false, !IsCurrentEraId(10));
            else if(dbEventInfo.EffectVal1 == (int) ActionTypes.SWAP_DEITIES_LEVELS)
                return player.PlayerDeities.HasTwoDeitiesWithDifferentLevels();
            else if(dbEventInfo.EffectVal1 == (int) ActionTypes.LOOSE_CITY)
            {
                int iNumCities = _gameContext.BoardManager.GetNumCities(player.Id);
                if(iNumCities == 1)
                {
                    player.LogAction(ActionTypes.LOOSE_CITY);
                    _gameContext.BoardManager.CityLost(player, _gameContext.BoardManager.GetFirstPlayerCity(player.Id));
                    return false;
                }
                else
                    return iNumCities > 0;
            }
            else if(dbEventInfo.EffectVal1 == (int) ActionTypes.SPECIALISTS_INTO_POINTS)
                return true;
            
            // special forced do not do if only 1 choice

            Console.WriteLine("Undefined action check " + iEventCard.ToString());
            return dbEventInfo.EffectVal2 == 1;
        }

        public bool IsCurrentEraId(int iCheck)
        {
            return iCheck == _gameContext.EraEffectManager.CurrentAgeCardId;
        }
        public bool HasAnyValidTileCity(Guid playerId, bool bFriendly, bool bEnemy, bool bNeutral, bool bRequiresAdjacentFriendly = false)
        {
            foreach(var tile in _gameContext.BoardManager.Tiles)
            {
                if(!tile.Jungle && tile.dbData.TileTypeId == 3)
                {
                    if((bFriendly && (tile.gameData.OwnerId == playerId)) || (bEnemy && (tile.gameData.OwnerId != playerId) && (tile.gameData.OwnerId != Guid.Empty)) || (bNeutral && (tile.gameData.OwnerId == Guid.Empty)))
                    {
                        if(!bRequiresAdjacentFriendly || HasUserCityAdjacentTo(playerId, tile))
                            return true;
                    }
                }
            }
            return false;
        }
        public bool HasUserCityAdjacentTo(Guid playerId, Tile tile)
        {
            foreach(var pTile in _gameContext.BoardManager.GetTilesInRange(tile, 1))
            {
                if(pTile.gameData.OwnerId == playerId)
                    return true;
            }
            return false;
        }
        public bool HasPlayerAtLeastOneCity(Guid player, bool bWithoutStelae = false, bool bWithStelae = false)
        {
            foreach(var tile in _gameContext.BoardManager.Tiles)
            {
                if(!tile.Jungle && tile.dbData.TileTypeId == 3)
                {
                    if(tile.gameData.OwnerId == player)
                    {
                        if(!bWithoutStelae || (tile.gameData.RulerStelae == -1))
                        {
                            if(!bWithStelae || (tile.gameData.RulerStelae != -1))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool CanMakeAnyAngle(Guid playerid)
        {
            foreach(var tile in _gameContext.BoardManager.Tiles)
            {
                if(!tile.Jungle && tile.dbData.TileTypeId == 3)
                {
                    if(tile.gameData.OwnerId == playerid)
                    {
                        if(CanPlayerMakeANewAngleFromTile(playerid, tile))
                            return true;
                    }
                }
            }
            return false;
        }
        public bool CanPlayerMakeANewAngleFromTile(Guid playerid, Tile tile)
        {
            var player = _gameContext.PlayerManager.GetPlayerById(playerid);
            foreach(var pTile in _gameContext.BoardManager.GetTilesInRange(tile, 2))
            {
                if(pTile.dbData.Id != tile.dbData.Id)
                {
                    if(_gameContext.ActionManager.CanPlayerTargetTileForAngle(player, pTile.dbData.Id))
                    {
                        int iAngle = _gameContext.BoardManager.GetPlayerBoardAngleBetween(player, tile.dbData.Id, pTile.dbData.Id);
                        if(iAngle != -1)
                        {
                            if(!player.PlayerAngleBoard.GetAngleById(iAngle).bChecked)
                                return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
