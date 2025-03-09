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
        public BoardManager BoardManager { get; set; }
        public PhaseManager PhaseManager { get; set; }
        public ActionManager ActionManager { get; set; }
        public EventsInGameManager EventsInGameManager { get; set; }
        public EraEffectManager EraEffectManager { get; set; }
        public TimerManager TimerManager { get; private set; }
        public ActionCardManager ActionCardManager {get; set;}
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
            PhaseManager = new PhaseManager(this);
            ActionCardManager = new ActionCardManager(this);
            TimerManager = new TimerManager(this);
            KonstelacjeManager = new KonstelacjeManager(this);
            EraEffectManager = new EraEffectManager(this);
            EventsInGameManager = new EventsInGameManager(this);
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
            StartOfGame data = new StartOfGame
            {
                GameId = GameId,
                Players = playerViewModels,
                PlayerDataChanges = ActionManager.GetListOfPlayersSetData(),
                FullRulerData = fff,
                PhaseData = PhaseManager.GetPhaseData(),
                Konstelacje = KonstelacjeManager.GetFullData(),
                startGameModel = GameOptions,
                EraEffects = EraEffectManager.GetFullData(),
                EventsLists = EventsInGameManager.GetFullData()
            };
            EventManager.Broadcast("GameStarted", ref data);
            ActionManager.ActionInitialized();

            PhaseManager.DoCheckCurrentPhase();
        }
    }
}