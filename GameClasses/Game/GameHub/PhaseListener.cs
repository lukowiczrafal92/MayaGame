using BoardGameBackend.Providers;
using BoardGameBackend.Models;
using Microsoft.AspNetCore.SignalR;

namespace BoardGameBackend.Managers.EventListeners
{
    public class PhaseEventListener : IEventListener
    {
        private readonly IHubContextProvider _hubContextProvider;

        public PhaseEventListener(IHubContextProvider hubContextProvider)
        {
            _hubContextProvider = hubContextProvider;
        }

        public void SubscribeEvents(GameContext gameContext)
        {
            var gameId = gameContext.GameId;

            gameContext.EventManager.Subscribe<DummyPhaseStarted>("HeroCardPickingPhaseStarted", data =>
            {
                BroadcastHeroCardPickingPhaseStart(gameContext.GameId, data);
            });

            gameContext.EventManager.Subscribe<DummyPhaseStarted>("DummyPhasePickingPhaseStarted", args =>
           {
               BroadcastDummyPickingPhaseStart(args.Player, gameId);
           });
            gameContext.EventManager.Subscribe<DummyPhaseStarted>("BoardPhasePickingPhaseStarted", args =>
            {
                BroadcastBoardPickingPhaseStart(args.Player, gameId);
            });
            gameContext.EventManager.Subscribe<DummyPhaseStarted>("ArtifactPhaseStarted", data =>
            {
                BroadcastArtifactPickingPhaseStart(gameId, data);
            });

            gameContext.EventManager.Subscribe<DummyPhaseStarted>("MercenaryPhaseStarted", args =>
            {
                BroadcastMercenaryPhaseStart(args.Player, gameId);
            });

            gameContext.EventManager.Subscribe<HeroTurnEnded>("HeroTurnEnded", args =>
            {
                BroadcastHeroTurnEnded(args.Player, gameId);
            }, priority: 20);

            gameContext.EventManager.Subscribe<PlayerInGame>("New player turn", player =>
            {
                BroadcastNewPlayerTurn(player, gameId);
            }, priority: 2);

            gameContext.EventManager.Subscribe<EndOfTurnEventData>("EndOfTurn", (data) =>
            {
                BroadcastEndOfTurn(gameId, data);
            });

            gameContext.EventManager.Subscribe("EndOfPlayerTurn", (EndOfPlayerTurn data) =>
            {
                BroadcastEndOfPlayerTurn(gameId, data);
            });

            gameContext.EventManager.Subscribe("GameStarted", (StartOfGame data) =>
            {
                BroadCastStartOfTheGame(data);
            }, priority: 0);

            gameContext.EventManager.Subscribe("PhaseStarted", (PhaseSendData data) =>
            {
                BroadCastPhaseSignal(gameId, data);
            }, priority: 0);

            gameContext.EventManager.Subscribe("SimpleChanges", (SimpleSendData data) =>
            {
                BroadCastSimpleChangeSignal(gameId, data);
            }, priority: 0);


            gameContext.EventManager.Subscribe("EraStartChanges", (EraStartSendData data) =>
            {
                BroadCastEraStartChangesSignal(gameId, data);
            }, priority: 0);
            
        }

        public void BroadcastHeroCardPickingPhaseStart(string gameId, DummyPhaseStarted data)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("HeroCardPickingPhaseStarted", data);
        }

        public void BroadcastDummyPickingPhaseStart(PlayerInGame player, string gameId)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("DummyPhaseStarted", player);
        }

        public void BroadcastMercenaryPhaseStart(PlayerInGame player, string gameId)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("MercenaryPhaseStarted", player);
        }


        public void BroadcastArtifactPickingPhaseStart(string gameId, DummyPhaseStarted data)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("ArtifactPhaseStarted", data);
        }

        public void BroadcastBoardPickingPhaseStart(PlayerInGame player, string gameId)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("BoardPhaseStarted", player);
        }

        public void BroadcastNewPlayerTurn(PlayerInGame player, string gameId)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("NewPlayerTurn", player);
        }

        public void BroadcastHeroTurnEnded(PlayerInGame player, string gameId)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("HeroTurnEnded", player);
        }

        public void BroadcastEndOfTurn(string gameId, EndOfTurnEventData data)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("EndOfTurn", data);
        }

        public void BroadcastEndOfPlayerTurn(string gameId, EndOfPlayerTurn data)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("EndOfPlayerTurn", data);
        }
        public void BroadCastStartOfTheGame(StartOfGame startOfGame){
           var hubContext = _hubContextProvider!.LobbyHubContext;     
           hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(startOfGame.GameId)!.Id).SendAsync("GameStarted", startOfGame);
        }

        public void BroadCastPhaseSignal(string gameId, PhaseSendData data){
           var hubContext = _hubContextProvider!.LobbyHubContext;     
           hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("PhaseStarted", data);
        }
        public void BroadCastSimpleChangeSignal(string gameId, SimpleSendData data){
           var hubContext = _hubContextProvider!.LobbyHubContext;     
           hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("SimpleChanges", data);
        }

        public void BroadCastEraStartChangesSignal(string gameId, EraStartSendData data){
           var hubContext = _hubContextProvider!.LobbyHubContext;     
           hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("EraStartChanges", data);
        }
    }
}