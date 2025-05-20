using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class BoardManager
    {
        public List<Tile> Tiles { get; private set; }
        private readonly GameContext _gameContext;
        public BoardManager(GameContext gameContext, int iNumPlayers)
        {
            _gameContext = gameContext;
            Tiles = new List<Tile>();
            List<TileGameData> l = GameDataManager.GetTiles();
            for(int i = 0; i < l.Count; i++)
                Tiles.Add(new Tile(l[i], iNumPlayers));
        }
        public BoardManager(GameContext gameContext, int iNumPlayers, List<TileGame> ftd)
        {
            _gameContext = gameContext;
            Tiles = new List<Tile>();
            List<TileGameData> l = GameDataManager.GetTiles();
            for(int i = 0; i < l.Count; i++)
            {
                Tiles.Add(new Tile(l[i], iNumPlayers));
                Tiles[i].gameData = ftd[i];
            }
        }

        public Tile? GetTileById(int id)
        {
            return Tiles.FirstOrDefault(p => p.dbData.Id == id);
        }
        public Tile? GetTileByXY(int iX, int iY)
        {
            return Tiles.FirstOrDefault(p => p.dbData.MapX == iX && p.dbData.MapY == iY);
        }

        public List<TileGame> GetFullTilesData()
        {
            List<TileGame> l = new List<TileGame>();
            for(int i = 0; i < Tiles.Count(); i++)
                l.Add(Tiles[i].gameData);

            return l;
        }

        public bool IsThereAtLeastOneNeutralCityLocation()
        {
            foreach(var tile in Tiles)
            {
                if(!tile.Jungle && tile.dbData.TileTypeId == 3)
                {
                    if(tile.gameData.OwnerId == Guid.Empty)
                        return true;
                }
            }
            return false; 
        }
        public int GetPlayerBoardAngleBetween(PlayerInGame player, int fromtile, int totile)
        {
            var ftile = GetTileById(fromtile);
            var ttile = GetTileById(totile);
            if(ftile == null || ttile == null)
                return -1;

            return GetPlayerBoardAngleBetweenXY(player, ftile.dbData.MapX, ftile.dbData.MapY, ttile.dbData.MapX, ttile.dbData.MapY);
        }

        public int GetPlayerBoardAngleBetweenXY(PlayerInGame player, int fromMapX, int fromMapY, int ToMapX, int ToMapY)
        {
            int iDifX = ToMapX - fromMapX;
            int iDifY = ToMapY - fromMapY;
            if((iDifX + 10) % 2 == 1 && (fromMapX + 10) % 2 == 1)
                iDifY++;

            PlayerAngleBoardTile? vAnswer = player.PlayerAngleBoard.GetAngleByXY(iDifX, iDifY);
            if(vAnswer == null)
                return -1;

            if(player.VisionAngle != 0)
            {   
                int iAngle = (vAnswer.dbInfo.Angle - player.VisionAngle * 60 + 360) % 360;
                return player.PlayerAngleBoard.GetPlayerAngleBoardByAngleDistance(vAnswer.dbInfo.Distance, iAngle).dbInfo.Id;
            }

            return vAnswer.dbInfo.Id;
        }

        public List<int[]> GetAdjacentXYs(Tile tile)
        {
            if((tile.dbData.MapX + 300) % 2 == 1)
                return new List<int[]>(){new int[2]{0, 1}, new int[2]{1, 0}, new int[2]{1, -1}, new int[2]{0, -1}, new int[2]{-1, -1}, new int[2]{-1, 0}};

            return new List<int[]>(){new int[2]{0, 1}, new int[2]{1, 1}, new int[2]{1, 0}, new int[2]{0, -1}, new int[2]{-1, 0}, new int[2]{-1, 1}};
        }

        public bool IsInRange(int fromTileId, int toTileId, int iRange)
        {
            return GetTilesInRange(GetTileById(fromTileId), iRange, false).Contains(GetTileById(toTileId));
        }

        public List<Tile> GetTilesInRange(Tile fromTile, int iRange, bool bExcludeSelf = true)
        {
            List<Tile> inRange = new List<Tile>(){fromTile};
            for(int iT = 1; iT <= iRange; iT++)
            {
                List<Tile> nTiles = new List<Tile>();
                foreach(var itTile in inRange)
                {
                    foreach(var xy in GetAdjacentXYs(itTile))
                    {
                        Tile cTile = GetTileByXY(itTile.dbData.MapX + xy[0], itTile.dbData.MapY + xy[1]);
                        if((cTile != null) && !inRange.Contains(cTile))
                        {
                            nTiles.Add(cTile);
                        }
                    }
                }
                inRange.AddRange(nTiles);
            }

            if(bExcludeSelf)
                inRange.Remove(fromTile);

            return inRange;
        }

        public void CityLost(PlayerInGame player, int tileId)
        {
            Tile tile = GetTileById(tileId);
            if(tile.gameData.Level > 0)
                CapitalStatusChanged(player, tile, -1, false);

            tile.gameData.OwnerId = Guid.Empty;
            tile.gameData.Level = 0;
            _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource1, -1);
            _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource2, -1);
        //    _gameContext.PlayerManager.ChangeLuxuryAmount(player, tile.dbData.LuxuryId, -1);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = player.Id, DataType = PlayerBasicSetDataType.CityDestroyed, Value1 = tile.dbData.Id});
        }

        public void SetCityOwner(PlayerInGame player, Tile tile)
        {
            if(tile.gameData.OwnerId != Guid.Empty)
            {
                var oldplayer = _gameContext.PlayerManager.GetPlayerById(tile.gameData.OwnerId);
                _gameContext.PlayerManager.ChangeResourceIncome(oldplayer, tile.dbData.Resource1, -1);
                _gameContext.PlayerManager.ChangeResourceIncome(oldplayer, tile.dbData.Resource2, -1);
            //    _gameContext.PlayerManager.ChangeLuxuryAmount(oldplayer, tile.dbData.LuxuryId, -1);
            }
            tile.gameData.OwnerId = player.Id;
            tile.gameData.Level = 0;
            _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource1, 1);
            _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource2, 1);
        //    _gameContext.PlayerManager.ChangeLuxuryAmount(player, tile.dbData.LuxuryId, 1);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = player.Id, DataType = PlayerBasicSetDataType.CityClaim, Value1 = tile.dbData.Id});

            if(GetCapitalCity(player.Id) == null)
            {
                if(GetNumCities(player.Id) == 3) // change to 6
                {
                    _gameContext.PhaseManager.PhaseQueue.Insert(1, new Phase(){PhaseType = PhaseType.ChooseCapital, ActivePlayers = new List<Guid>(){player.Id}});
                }
            }
        }

        public void CityExpands(PlayerInGame player, int tileid)
        {
            var tile = GetTileById(tileid);
            tile.gameData.Level++;
            if(tile.gameData.Level == 1)
                CapitalStatusChanged(player, tile, 1, true);

            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = player.Id, DataType = PlayerBasicSetDataType.CityClaim, Value1 = tile.dbData.Id});
        }

        public void CityLoosesExpansion(PlayerInGame player, int tileid)
        {
            var tile = GetTileById(tileid);
            tile.gameData.Level--;
            if(tile.gameData.Level == 0)
                CapitalStatusChanged(player, tile, -1, false);

            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = player.Id, DataType = PlayerBasicSetDataType.CityLevel, Value1 = tile.dbData.Id, Value2 = tile.gameData.Level});
        }

        public void CapitalStatusChanged(PlayerInGame player, Tile tile, int iAdd, bool bAdd)
        {
            if(_gameContext.EraEffectManager.CurrentAgeCardId != 9)
            {
                _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource1, -1 * iAdd);
                _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource2, -1 *  iAdd);
            }
        
        //   nie ma sensu na razie
        //    _gameContext.PlayerManager.SetHasLuxuryAlways(player, tile.dbData.LuxuryId, iAdd);
        }

        public void CityFoundOrConquest(PlayerInGame player, int tileid)
        {
            SetCityOwner(player, GetTileById(tileid));
        }

        public Tile? GetCapitalCity(Guid playerid)
        {
            foreach(var tile in Tiles)
            {
                if(tile.gameData.OwnerId == playerid && tile.gameData.Level > 0)
                    return tile;
            }
            return null;
        }

        public int GetCapitalCityLevel(Guid playerid)
        {
            var tile = GetCapitalCity(playerid);
            if(tile == null)
                return 0;

            return tile.gameData.Level;
        }

        public int GetFirstPlayerCity(Guid playerId, bool bIgnoreCapital = false)
        {
            foreach(var tile in Tiles)
            {
                if(tile.gameData.OwnerId == playerId)
                {
                    if(!bIgnoreCapital || tile.gameData.Level < 1)
                        return tile.dbData.Id;
                }
            }
            return -1;
        }
        public int GetNumCities(Guid playerId)
        {
            int cc = 0;
            foreach(var tile in Tiles)
            {
                if(tile.gameData.OwnerId == playerId)
                    cc++;
            }
            return cc;
        }

        public bool HasCityWithLuxury(Guid playerId, int iLuxuryId)
        {
            foreach(var tile in Tiles)
            {
                if(tile.gameData.OwnerId == playerId)
                {
                    if(tile.dbData.LuxuryId == iLuxuryId)
                        return true;
                }
            }
            return false;
        }

        public int GetCityStackAmount(Guid playerId)
        {
            int iBigestStack = 0;
            Dictionary<int, bool> dChecked = new Dictionary<int, bool>();
            foreach(var tile in Tiles)
            {
                if(!dChecked.ContainsKey(tile.dbData.Id) && tile.gameData.OwnerId == playerId)
                {
                    int iCurrentStack = 1;
                    dChecked.Add(tile.dbData.Id, true);
                    List<Tile> tileIt = new List<Tile>(){tile};
                    for(int i = 0; i < 10; i++)
                    {
                        if(tileIt.Count >= i + 1)
                        {
                            foreach(var sTile in GetTilesInRange(tileIt[i], 1, true))
                            {
                                if(sTile.gameData.OwnerId == playerId && !dChecked.ContainsKey(sTile.dbData.Id))
                                {
                                    iCurrentStack++;
                                    dChecked.Add(sTile.dbData.Id, true);
                                    tileIt.Add(sTile);
                                }
                            }
                        }
                    }
                    if(iCurrentStack > iBigestStack)
                        iBigestStack = iCurrentStack;
                }
            }
            return iBigestStack;
        }
    }
}
