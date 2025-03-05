using BoardGameBackend.Providers;
using BoardGameBackend.Models;
using Microsoft.AspNetCore.SignalR;
using BoardGameBackend.Hubs;
using BoardGameBackend.Mappers;

namespace BoardGameBackend.Managers.EventListeners
{
    public class OtherEventListener : IEventListener
    {
        private readonly IHubContextProvider _hubContextProvider;

        public OtherEventListener(IHubContextProvider hubContextProvider)
        {
            _hubContextProvider = hubContextProvider;
        }

        public void SubscribeEvents(GameContext gameContext)
        {
            var gameId = gameContext.GameId;

               
            gameContext.EventManager.Subscribe<TeleportationData>("TeleportationEvent", teleportationData =>
            {             
                BroadcastTeleportation(gameId, teleportationData);                     
            }, priority: 5);

            gameContext.EventManager.Subscribe<EndOfGame>("EndOfGame", endOfGame =>
            {             
                BroadcastEndOfGame(gameId, endOfGame);                     
            }, priority: 1);
            
            gameContext.EventManager.Subscribe("ActionCardsGivenToPlayer", (ActionCardsPlayerData data) =>
            {
                BroadcastActionCardsGivenToPlayer(gameId, data, "ActionCardsGivenToPlayer");
            }, priority: 0);
        }

        public void BroadcastTeleportation(string gameId, TeleportationData teleportationData)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("TeleportationEvent", teleportationData);
        }

        public void BroadcastEndOfGame(string gameId, EndOfGame endOfGame)
        {
            var hubContext = _hubContextProvider!.LobbyHubContext;
            hubContext.Clients.Group(LobbyManager.GetLobbyByGameId(gameId)!.Id).SendAsync("EndOfGame", endOfGame);
        }
        public void BroadcastActionCardsGivenToPlayer(string gameId, ActionCardsPlayerData data,
        string EventForUserThatGetsArtifact)
        {
            IHubContext<LobbyHub> hubContext = _hubContextProvider!.LobbyHubContext;
            string connectionId = LobbyHub.ConnectionMappings
                .FirstOrDefault(kvp => kvp.Value.Id == data.Player).Key;

            var lobbyId = LobbyManager.GetLobbyByGameId(gameId)!.Id;

            hubContext.Clients.Client(connectionId)
                .SendAsync(EventForUserThatGetsArtifact, data);
        }
    }
}
