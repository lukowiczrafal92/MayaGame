using BoardGameBackend.GameData;
using BoardGameBackend.Helpers;
using BoardGameBackend.Models;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Managers
{
    public class PlayersManager
    {
        public List<PlayerInGame> Players { get; private set; }
        private readonly GameContext _gameContext;
        public PlayersManager(List<Player> players, GameContext gameContext)
        {
            _gameContext = gameContext;
            var random = new Random();
            Players = players
                .OrderBy(p => random.Next())
                .Select(p => new PlayerInGame(p))
                .ToList();

            int iFirstPlayerVision = random.Next(0,6);
            List<int> iVisionOffsets = GetInitVisionOffsets(Players.Count());
            for(int i = 0; i < Players.Count(); i++)
            {
                Players[i].CurrentOrder = i + 1;
                Players[i].SetVisionAngle((iFirstPlayerVision + iVisionOffsets[i]) % 6);
            }
        }

        public PlayerInGame? GetPlayerById(Guid playerId)
        {
            return Players.FirstOrDefault(p => p.Id == playerId);
        }

        public List<int> GetInitVisionOffsets(int iNumPlayers)
        {
            if(iNumPlayers == 2)
                return new List<int>(){0,3,0,0,0,0};
            else if(iNumPlayers == 3)
                return new List<int>(){0,2,4,0,0,0};
            else if(iNumPlayers == 4)
                return new List<int>(){0,1,3,4,0,0};
            else if(iNumPlayers == 5)
                return new List<int>(){0,1,2,3,4,0};

            return new List<int>(){0,1,2,3,4,5};
        }

        public List<PlayerInGame> GetPlayersInReverseOrder()
        {
            return  Players
                    .OrderByDescending(p => p.CurrentOrder)
                    .ToList();
        }
        public List<PlayerInGame> GetPlayersInOrder()
        {
            return  Players
                    .OrderBy(p => p.CurrentOrder)
                    .ToList();
        }

        public void ChangeResourceIncome(PlayerInGame p, int iResourceId, int iAmount)
        {
            p.PlayerResources.GetResourceById(iResourceId).Income += iAmount;
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ResourceIncomeAmount, Value1 = iResourceId, Value2 = p.PlayerResources.GetResourceById(iResourceId).Income});
        }
        public void ChangeResourceAmount(PlayerInGame p, int iResourceId, int iAmount)
        {
            p.PlayerResources.GetResourceById(iResourceId).Amount += iAmount;
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ResourceAmount, Value1 = iResourceId, Value2 = p.PlayerResources.GetResourceById(iResourceId).Amount});
        }

        public void SetResourceAmount(PlayerInGame p, int iResourceId, int iAmount)
        {
            if(p.PlayerResources.GetResourceById(iResourceId).Amount == iAmount)
                return;
                
            p.PlayerResources.GetResourceById(iResourceId).Amount = iAmount;
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ResourceAmount, Value1 = iResourceId, Value2 = p.PlayerResources.GetResourceById(iResourceId).Amount});
        }

        public void ClaimOrderForNextTurn(PlayerInGame p)
        {
            int iNextOrder = 0;
            if(_gameContext.EraEffectManager.CurrentAgeCardId == 3)
            {
                iNextOrder = Players.Count;
                foreach(var player in Players)
                {
                    if(player.IncomingOrder <= iNextOrder && player.IncomingOrder != -1)
                        iNextOrder = player.IncomingOrder - 1;
                }
            }
            else
            {
                foreach(var player in Players)
                {
                    if(player.IncomingOrder > iNextOrder)
                        iNextOrder = player.IncomingOrder;
                }
                iNextOrder++;
            }
            p.IncomingOrder = iNextOrder;
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.IncomingOrder, Value1 = p.IncomingOrder});
        }

        public void AddResourceConverter(PlayerInGame p, int iConverterId)
        {
            var dbData = GameDataManager.GetResourceConverterById(iConverterId);
            if(p.PlayerResources.GetResourceById(dbData.FromResource).Converters.Contains(iConverterId))
                return;

            p.PlayerResources.GetResourceById(dbData.FromResource).Converters.Add(iConverterId);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ResourceConverter, Value1 = iConverterId, Value2 = 1});
        }
        public bool HasAuraEffectId(Guid player, int effectId)
        {
            return HasPlayerAuraEffectId(GetPlayerById(player), effectId);
        }
        public bool HasPlayerAuraEffectId(PlayerInGame _player, int effectId)
        {
            return _player.AuraEffects.Contains(effectId);
        }

        public void ChangeWarfareScore(PlayerInGame p, int amount)
        {
            if(amount == 0) return;

            p.WarfareScore += amount;
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.WarfareScore, Value1 = p.WarfareScore});
        }

        public void ChangeScorePoints(PlayerInGame p, int amount, ScorePointType spType)
        {
            if(amount == 0) return;

            p.Points += amount;
            p.ChangeScoreSource(spType, amount);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ScorePoints, Value1 = p.Points});
        }

        public void ReduceDeityLevel(PlayerInGame p, int iDeityID)
        {
            var deity = p.PlayerDeities.GetDeityById(iDeityID);
            if(deity.Level > 0)
            {
                deity.Level--;
                _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.DeityLevel, Value1 = iDeityID, Value2 = p.PlayerDeities.GetDeityById(iDeityID).Level});
            }
        }
        public void IncreaseDeityLevel(PlayerInGame p, int iDeityID)
        {
            var deity = p.PlayerDeities.GetDeityById(iDeityID);
            if(deity.Level == 3) // NOW MAX LEVEL :>
            {
                ChangeScorePoints(p, GameConstants.DEITY_LVL_FIVE_POINTS, ScorePointType.DuringGameDeity);
            }   
            else
            {
                deity.Level++;
                if(deity.Level == 1)
                    ChangeResourceAmount(p, deity.Resource, 1);
                else if(deity.Level == 3)
                    ChangeLuxuryAmount(p, deity.Luxury, 1);
                    
                _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.DeityLevel, Value1 = iDeityID, Value2 = p.PlayerDeities.GetDeityById(iDeityID).Level});
            }         
        }

        public void ChangeLuxuryAmount(PlayerInGame p, int iLuxuryId, int iAmount)
        {
            p.PlayerLuxuries.GetLuxuryById(iLuxuryId).Amount += iAmount;

            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.Luxury, Value1 = iLuxuryId, Value2 = p.PlayerLuxuries.GetLuxuryById(iLuxuryId).Amount});
        }

        public void CheckAngle(PlayerInGame p, int angle)
        {
            bool bAnyPlayerHasAngle = false;
            foreach(var player in Players)
            {
                if(player.PlayerAngleBoard.GetAngleById(angle).bChecked)
                {
                    bAnyPlayerHasAngle = true;
                    break;
                }
            }
            if(!bAnyPlayerHasAngle)
                ChangeScorePoints(p, 1, ScorePointType.DuringGameAngle);
            
            p.PlayerAngleBoard.GetAngleById(angle).bChecked = true;
            
            // tutaj konstelacje
            // tutaj mark angleendscoreifever            
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.BoardAngle, Value1 = angle});

            _gameContext.KonstelacjeManager.CheckNewAngleByPlayer(p, angle);
        }
        public void UsedActionCardFromHand(PlayerInGame p, int gameindexcard)
        {
            var card = p.HandActionCards.FirstOrDefault(c => c.GameIndex == gameindexcard);
            if(card != null)
            {
                p.HandActionCards.Remove(card);
                _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.UsedActionCard, Value1 = gameindexcard});
            }
        }
        public void TriggerIncome(PlayerInGame p)
        {
            foreach(var resource in p.PlayerResources.Resources)
            {
                if(resource.Income != 0)
                    ChangeResourceAmount(p, resource.Id, resource.Income);
            }
        }

        public void ApplyResourceConverter(PlayerInGame p, ResourceConverterGameData data)
        {
            ChangeResourceAmount(p, data.FromResource, -1 * data.FromValue);
            ChangeResourceAmount(p, data.ToResource, data.ToValue);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){DataType = PlayerBasicSetDataType.LogRespecialization, Player = p.Id, Value1 = data.Id});
        }

        public bool HasNeedOfEndGameConverting(PlayerInGame p)
        {
            bool bAnyConverterUsable = false;
            bool bAnyConverterIntoScorable = false;
            bool bAnyScoreResource = false; 
            foreach(var resource in p.PlayerResources.Resources)
            {
                if(resource.EndGameScore > 0)
                    bAnyScoreResource = true;

                foreach(var converter in resource.Converters)
                {
                    var dbinfo = GameDataManager.GetResourceConverterById(converter);
                    if(dbinfo.FromValue <= resource.Amount)
                    {
                        bAnyConverterUsable = true;
                        if(p.PlayerResources.GetResourceById(dbinfo.ToResource).EndGameScore > 0)
                            bAnyConverterIntoScorable = true;
                    }
                }
            }
            return bAnyConverterUsable && bAnyScoreResource && bAnyConverterIntoScorable;
        }
        public bool HasNeedOfExtraConverting(PlayerInGame p)
        {
            if(_gameContext.EraEffectManager.CurrentAgeCardId == 2)
            {
                TrimResourceOverStorage(p);
                return false;
            }
            bool bNeed = false;
            foreach(var resource in p.PlayerResources.Resources)
            {
                if(resource.Amount > GameConstants.MAX_RESOURCE_STORAGE)
                {
                    if(resource.Converters.Count > 0)
                        bNeed = true;
                    else
                        SetResourceAmount(p, resource.Id, GameConstants.MAX_RESOURCE_STORAGE);
                }
            }
            return bNeed;
        }

        public void TrimResourceOverStorage(PlayerInGame p)
        {
            foreach(var resource in p.PlayerResources.Resources)
            {
                if(resource.Amount > GameConstants.MAX_RESOURCE_STORAGE)
                {
                    SetResourceAmount(p, resource.Id, GameConstants.MAX_RESOURCE_STORAGE);
                }
            }
        }

        public void RefreshOrder()
        {
            foreach(var player in Players)
            {
                player.CurrentOrder = player.IncomingOrder;
                player.IncomingOrder = -1;
            }
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){DataType = PlayerBasicSetDataType.PlayerOrder, Player = Guid.Empty});
        }

        public void AddEffectId(PlayerInGame player, int effectId)
        {
            player.AuraEffects.Add(effectId);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){DataType = PlayerBasicSetDataType.AuraEffect, Player = player.Id, Value1 = effectId});
        }

        public void RemoveEffectId(PlayerInGame player, int effectId)
        {
            player.AuraEffects.Remove(effectId);
            _gameContext.ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){DataType = PlayerBasicSetDataType.RemoveAuraEffect, Player = player.Id, Value1 = effectId});
        }
        
        public bool HasPlayerRulerWithAngleId(PlayerInGame player, int angleid)
        {
            foreach(var ruler in player.Rulers)
            {
                if(ruler.dbInfo.Angle == angleid)
                    return true;
            }
            return false;
        }
    }
}
