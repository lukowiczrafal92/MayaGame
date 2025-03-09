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

        public void  SetCityOwner(PlayerInGame player, Tile tile)
        {
            if(tile.gameData.OwnerId != Guid.Empty)
            {
                var oldplayer = _gameContext.PlayerManager.GetPlayerById(tile.gameData.OwnerId);
                _gameContext.PlayerManager.ChangeResourceIncome(oldplayer, tile.dbData.Resource1, -1);
                _gameContext.PlayerManager.ChangeResourceIncome(oldplayer, tile.dbData.Resource2, -1);
                _gameContext.PlayerManager.ChangeLuxuryAmount(oldplayer, tile.dbData.LuxuryId, -1);
            }
            tile.gameData.OwnerId = player.Id;
            tile.gameData.Level = 0;
            _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource1, 1);
            _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource2, 1);
            _gameContext.PlayerManager.ChangeLuxuryAmount(player, tile.dbData.LuxuryId, 1);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = player.Id, DataType = PlayerBasicSetDataType.CityClaim, Value1 = tile.dbData.Id});
        }

        public void CityExpands(PlayerInGame player, int tileid)
        {
            var tile = GetTileById(tileid);
            tile.gameData.Level++;
            if(tile.gameData.Level == 1)
                CapitalStatusChanged(player, tile, 1, true);
//            _gameContext.PlayerManager.ChangeScorePoints(player, tile.gameData.Level, ScorePointType.DuringGameCapitalCity);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = player.Id, DataType = PlayerBasicSetDataType.CityClaim, Value1 = tile.dbData.Id});
        }

        public void CapitalStatusChanged(PlayerInGame player, Tile tile, int iAdd, bool bAdd)
        {
            if(_gameContext.EraEffectManager.CurrentAgeCardId == 9)
            {
                _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource1, iAdd);
                _gameContext.PlayerManager.ChangeResourceIncome(player, tile.dbData.Resource2, iAdd);
            }
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
    }
}
