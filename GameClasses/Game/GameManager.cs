using System.Collections.Concurrent;
using BoardGameBackend.Managers.EventListeners;
using BoardGameBackend.Mappers;
using BoardGameBackend.Models;
using BoardGameBackend.Providers;
using BoardGameBackend.Repositories;

namespace BoardGameBackend.Managers
{
    public static class GameManager
    {
        private static readonly ConcurrentDictionary<string, GameContext> ActiveGames = new ConcurrentDictionary<string, GameContext>();
        private static readonly ConcurrentDictionary<string, EventListenerContainer> EventListenerContainers = new();
        private static IHubContextProvider? _hubContextProvider;
        private static IGameBackupSaver? _gameBackupSaver;

        public static void Initialize(IHubContextProvider hubContextProvider, IGameBackupSaver gameBackupSaver)
        {
            _hubContextProvider = hubContextProvider;
            _gameBackupSaver = gameBackupSaver;
        }

        public static GameContext StartGameFromBackup(Lobby lobby, StartGameModel startGameModel, FullGameBackup fullGameBackup, string gameId)
        {
            var players = lobby.Players.Select(p => new Player { Id = p.Id, Name = p.Name }).ToList();

            var gameContext = new GameContext(gameId, players, startGameModel, fullGameBackup);
            
            DoCommonGameContextStart(gameContext, gameId);
            return gameContext;
        }

        public static void DoCommonGameContextStart(GameContext gameContext, string gameId)
        {
            var eventListenerContainer = new EventListenerContainer(_hubContextProvider);
            eventListenerContainer.SubscribeAll(gameContext);
            
            ActiveGames.TryAdd(gameId, gameContext);
            EventListenerContainers.TryAdd(gameId, eventListenerContainer);

            gameContext.EventManager.Subscribe<EndOfGame>("EndOfGame", endOfGame =>
            {             
                EndGame(endOfGame.GameId);                     
            }, priority: 0);

            gameContext.EventManager.Subscribe<RequestBackupData>("GameRequestBackup", pData =>
            {             
                BackupRequested(pData.GameId);                     
            }, priority: 0);
        }

        public static GameContext StartGameFromLobby(Lobby lobby, StartGameModel startGameModel)
        {
            var gameId = Guid.NewGuid().ToString();
            var players = lobby.Players.Select(p => new Player { Id = p.Id, Name = p.Name }).ToList();

            var gameContext = new GameContext(gameId, players, startGameModel);
            
            DoCommonGameContextStart(gameContext, gameId);
            return gameContext;
        }

        public static GameContext? GetGameById(string gameId)
        {
            if (ActiveGames.TryGetValue(gameId, out var gameContext))
            {
                return gameContext;
            }

            return null;
        }

        public static FullGameBackup GetGameBackupFullData(string gameId)
        {
            var gameContext = GetGameById(gameId);
            if(gameContext == null) return null;

            
            List<FullPlayerData> playerData =  new List<FullPlayerData>{}; 
            List<PlayerBasicSetData> nSetData = new List<PlayerBasicSetData>();
            FillPlayerData(playerData, gameContext, nSetData);

            FullGameBackup FullGameBackup = new FullGameBackup()
            {
                PlayersData = playerData,
                TilesData = gameContext.BoardManager.GetFullTilesData(),
                PlayerSetData = nSetData,
                FullRulerData = gameContext.RulerCardsManager.GetFullRulerBackup(),
                PhaseData = gameContext.PhaseManager.GetPhaseData(),
                Konstelacje = gameContext.KonstelacjeManager.GetFullData(),
                EraEffects = gameContext.EraEffectManager.GetFullData(),
                EventsLists = gameContext.EventsInGameManager.GetFullData(),
                PhaseQueue = gameContext.PhaseManager.PhaseQueue,
                ActionDeck = gameContext.ActionCardManager.GetActionCards(),
                PlayerActionCards = gameContext.PlayerManager.GetBackupActionCards()
            };
            return FullGameBackup;
        }

        public static FullGameData? GetGameData(string gameId, Guid playerId)
        {
            Console.WriteLine("Gracz wraca do gry " + gameId);
            var gameContext = GetGameById(gameId);

            if(gameContext == null) return null;
            List<FullPlayerData> playerData =  new List<FullPlayerData>{}; 
            List<PlayerBasicSetData> nSetData = new List<PlayerBasicSetData>();
            FillPlayerData(playerData, gameContext, nSetData);
            FullRulerData FullRulerData = new FullRulerData()
                {DeckAmount = gameContext.RulerCardsManager.GetRulersDeckAmount(), 
                RulerPool = gameContext.RulerCardsManager.GetRulerPool()};
            var fullGameData = new FullGameData {
                GameId = gameId,
                PlayersData = playerData,
                TilesData = gameContext.BoardManager.GetFullTilesData(),
                PlayerSetData = nSetData,
                FullRulerData = FullRulerData,
                PhaseData = gameContext.PhaseManager.GetPhaseData(),
                PlayerActionCards = gameContext.ActionCardManager.GetPlayerActioncCardsData(gameContext.PlayerManager.GetPlayerById(playerId)),
                Konstelacje = gameContext.KonstelacjeManager.GetFullData(),
                startGameModel = gameContext.GameOptions,
                EraEffects = gameContext.EraEffectManager.GetFullData(),
                EventsLists = gameContext.EventsInGameManager.GetFullData()
            };

            return fullGameData;
        }

        public static void FillPlayerData(List<FullPlayerData> playersData, GameContext gameContext, List<PlayerBasicSetData> nSetData){
            gameContext.PlayerManager.Players.ForEach(p => {
                FullPlayerData playerData = new FullPlayerData {
                    Player = new PlayerViewModelData{Name = p.Name, Id = p.Id, VisionAngle = p.VisionAngle, CurrentOrder = p.CurrentOrder, IncomingOrder = p.IncomingOrder}
                };
                playersData.Add(playerData);

                if(p.Points != 0)
                    nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ScorePoints, Value1 = p.Points});

                if(p.WarfareScore != 0)
                    nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.WarfareScore, Value1 = p.WarfareScore});

                foreach(var angleData in p.PlayerAngleBoard.Angles)
                {
                    if(angleData.bChecked)
                        nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.BoardAngle, Value1 = angleData.dbInfo.Id});           
                }

                foreach(var luxuryData in p.PlayerLuxuries.Luxuries)
                {
                    if(luxuryData.Amount > 0)
                        nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.Luxury, Value1 = luxuryData.Id, Value2 = luxuryData.Amount}); 
                }

                foreach(var deity in p.PlayerDeities.Deities)
                {
                    if(deity.Level != 0)
                        nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.DeityLevel, Value1 = deity.Id, Value2 = deity.Level}); 
                }

                foreach(var ruler in p.Rulers)
                    nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.RulerCard, Value1 = ruler.dbInfo.Id});

                foreach(var effectid in p.AuraEffects)
                    nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.AuraEffect, Value1 = effectid});

                foreach(var resource in p.PlayerResources.Resources)
                {
                    if(resource.Amount != 0)
                        nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ResourceAmount, Value1 = resource.Id, Value2 = resource.Amount});

                    if(resource.Income != 0)
                        nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ResourceIncomeAmount, Value1 = resource.Id, Value2 = resource.Income});

                    foreach(var converter in resource.Converters)
                        nSetData.Add(new PlayerBasicSetData(){Player = p.Id, DataType = PlayerBasicSetDataType.ResourceConverter, Value1 = converter, Value2 = 1});
                }
            });
            // handled in GetFullTilesData
       /*     foreach(var tile in gameContext.BoardManager.Tiles)
            {
                if(tile.gameData.OwnerId != Guid.Empty)
                    nSetData.Add(new PlayerBasicSetData(){Player = tile.gameData.OwnerId, DataType = PlayerBasicSetDataType.CityClaim, Value1 = tile.dbData.Id, Value2 = tile.gameData.Level});
            } */
        }

        public static void EndGame(string gameId)
        {
            Console.WriteLine("Próbujemy usunąć grę: " + gameId);
            ActiveGames.TryRemove(gameId, out _);
            _gameBackupSaver.DeleteGameId(gameId);
        }

        public static void BackupRequested(string gameId)
        {            
            FullGameBackup fgb = GetGameBackupFullData(gameId);
            if(fgb == null) return;
            
            _gameBackupSaver.UpdateBackup(gameId, fgb);
        }

    }
}