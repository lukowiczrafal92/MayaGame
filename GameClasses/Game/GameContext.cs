using AutoMapper;
using BoardGameBackend.GameData;
using BoardGameBackend.Mappers;
using BoardGameBackend.Models;

namespace BoardGameBackend.Managers
{
    public class GameContext
    {
        public string GameId { get; private set; }
        public StartGameModel GameOptions { get; private set; }
        public PlayersManager PlayerManager { get; private set; }
        public EventManager EventManager { get; }
        public int iLuxuryBonus {get; set;}
        public BoardManager BoardManager { get; set; }
        public PhaseManager PhaseManager { get; set; }
        public ActionManager ActionManager { get; set; }
        public EventsInGameManager EventsInGameManager { get; set; }
        public EraEffectManager EraEffectManager { get; set; }
        public TimerManager TimerManager { get; private set; }
        public ActionCardManager ActionCardManager {get; set;}
        public StolicaCardsManager StolicaCardsManager {get; set;}
        public KonstelacjeManager KonstelacjeManager {get; set;}
        public ScorePointsManager ScorePointsManager { get; private set; }
        public RulerCardsManager RulerCardsManager {get; set; }
        public GameContext(string gameId, List<Player> players, StartGameModel startGameModel)
        {
            EventManager = new EventManager();
            GameId = gameId;
            GameOptions = startGameModel;
            ActionManager = new ActionManager(this);
            PlayerManager = new PlayersManager(players, this);
            BoardManager = new BoardManager(this, players.Count());
            ScorePointsManager = new ScorePointsManager(this);
            RulerCardsManager = new RulerCardsManager(this);
            StolicaCardsManager = new StolicaCardsManager(this);
            PhaseManager = new PhaseManager(this);
            ActionCardManager = new ActionCardManager(this);
            TimerManager = new TimerManager(this);
            KonstelacjeManager = new KonstelacjeManager(this);
            EraEffectManager = new EraEffectManager(this);
            EventsInGameManager = new EventsInGameManager(this);
        }
        public GameContext(string gameId, List<Player> players, StartGameModel startGameModel, FullGameBackup fullGameBackup)
        {
            EventManager = new EventManager();
            GameId = gameId;
            GameOptions = startGameModel;
            ActionManager = new ActionManager(this);
            PlayerManager = new PlayersManager(players, fullGameBackup.PlayersData, fullGameBackup.PlayerActionCards, this);
            BoardManager = new BoardManager(this, players.Count(), fullGameBackup.TilesData);
            ScorePointsManager = new ScorePointsManager(this);
            RulerCardsManager = new RulerCardsManager(this, fullGameBackup.FullRulerData);
            StolicaCardsManager = new StolicaCardsManager(this, fullGameBackup.FullStolicaData);
            PhaseManager = new PhaseManager(this, fullGameBackup.PhaseData, fullGameBackup.PhaseQueue);
            ActionCardManager = new ActionCardManager(this, fullGameBackup.ActionDeck);
            TimerManager = new TimerManager(this); // wiadomo tego nie ruszamy :) czasy się zresetują i tyle
            KonstelacjeManager = new KonstelacjeManager(this, fullGameBackup.Konstelacje);
            EraEffectManager = new EraEffectManager(this, fullGameBackup.EraEffects);
            EventsInGameManager = new EventsInGameManager(this, fullGameBackup.EventsLists);
            iLuxuryBonus = fullGameBackup.LuxuryBonus;
            RepopulateBackupPlayerData(fullGameBackup.PlayerSetData);
        }
        public void RepopulateBackupPlayerData(List<PlayerBasicSetData> actions)
        {
            foreach(var action in actions)
            {
                if(action.Player != Guid.Empty)
                {
                    var _player = PlayerManager.GetPlayerById(action.Player);
                    if(_player == null)
                        continue;

                    if(action.DataType == PlayerBasicSetDataType.BoardAngle)
                        _player.PlayerAngleBoard.GetAngleById(action.Value1).bChecked = true;
                    else if(action.DataType == PlayerBasicSetDataType.ResourceAmount)
                        _player.PlayerResources.GetResourceById(action.Value1).Amount = action.Value2;
                    else if(action.DataType == PlayerBasicSetDataType.ResourceIncomeAmount)
                        _player.PlayerResources.GetResourceById(action.Value1).Income = action.Value2;
                    else if(action.DataType == PlayerBasicSetDataType.ResourceConverter)
                    {
                        if(action.Value2 == 1)
                        {
                            var dbInfo = GameDataManager.GetResourceConverterById(action.Value1);
                            _player.PlayerResources.GetResourceById(dbInfo.FromResource).Converters.Add(action.Value1);
                        }
                    }
                    else if(action.DataType == PlayerBasicSetDataType.DeityLevel)
                        _player.PlayerDeities.GetDeityById(action.Value1).Level = action.Value2;
                    else if(action.DataType == PlayerBasicSetDataType.RulerCard)
                        RulerCardsManager.PlayerBackupRulerCard(_player, action.Value1);
                    else if(action.DataType == PlayerBasicSetDataType.CapitalCard)
                        _player.Stolica = action.Value1;
                    else if(action.DataType == PlayerBasicSetDataType.ScorePoints)
                        _player.Points = action.Value1;
                    else if(action.DataType == PlayerBasicSetDataType.WarfareScore)
                        _player.WarfareScore = action.Value1;
                    else if(action.DataType == PlayerBasicSetDataType.Luxury)
                        _player.PlayerLuxuries.GetLuxuryById(action.Value1).Amount = action.Value2;
                    else if(action.DataType == PlayerBasicSetDataType.HasLuxury)
                        _player.PlayerLuxuries.GetLuxuryById(action.Value1).HasLuxury = true;
                    else if(action.DataType == PlayerBasicSetDataType.LuxuryPermanent)
                        _player.PlayerLuxuries.GetLuxuryById(action.Value1).AlwaysHasLuxury = action.Value2;
                    else if(action.DataType == PlayerBasicSetDataType.AuraEffect)
                        _player.AuraEffects.Add(action.Value1);
                    else if(action.DataType == PlayerBasicSetDataType.EstScoreEnd)
                        _player.EndPoints = action.Value1;
                }
            }
        }

        public void StartGame()
        {
            var playerViewModels = PlayerManager.Players.Select(GameMapper.Instance.Map<PlayerViewModelData>).ToList();
            // initial resources
            foreach(var resource in GameDataManager.GetResources())
            {
                if(resource.StartingValue != 0)
                {
                    foreach(var p in PlayerManager.Players)
                        PlayerManager.ChangeResourceAmount(p, resource.Id, resource.StartingValue);
                }
            }
            foreach(var converter in GameDataManager.GetResourceConverters())
            {
                if(converter.Initial)
                {
                    foreach(var p in PlayerManager.Players)
                        PlayerManager.AddResourceConverter(p, converter.Id);

                }
            }


            FullRulerData fff = new FullRulerData(){
                DeckAmount = RulerCardsManager.GetRulersDeckAmount(), 
                RulerPool = RulerCardsManager.GetRulerPool()};

            iLuxuryBonus = PlayerManager.Players.Count * 2;
            StartOfGame data = new StartOfGame
            {
                GameId = GameId,
                Players = playerViewModels,
                PlayerDataChanges = ActionManager.GetListOfPlayersSetData(),
                FullRulerData = fff,
                PhaseData = PhaseManager.GetPhaseData(),
                StoliceFullData = StolicaCardsManager.GetFullStolicaBackup(),
                Konstelacje = KonstelacjeManager.GetFullData(),
                startGameModel = GameOptions,
                EraEffects = EraEffectManager.GetFullData(),
                EventsLists = EventsInGameManager.GetFullData(),
                LuxuryBonus = iLuxuryBonus
            };
            EventManager.Broadcast("GameStarted", ref data);
            ActionManager.ActionInitialized();
            PhaseManager.DoCheckCurrentPhase();
        }

        public void DoReduceLuxuryBonus()
        {
            if(iLuxuryBonus > PlayerManager.Players.Count)
            {
                iLuxuryBonus--;
                ActionManager.AddPlayerBasicSetData(new PlayerBasicSetData(){Player = Guid.Empty, DataType = PlayerBasicSetDataType.GameLuxuryBonus, Value1 = iLuxuryBonus});
            }
        }

        public void CreateBackupLobbyGame()
        {
            RequestBackupData data = new RequestBackupData(){GameId = GameId};
            EventManager.Broadcast("GameRequestBackup", ref data);
        }
    }
}