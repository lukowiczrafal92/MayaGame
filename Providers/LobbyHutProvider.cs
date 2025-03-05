using BoardGameBackend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BoardGameBackend.Providers{
    public interface IHubContextProvider
    {
        IHubContext<LobbyHub> LobbyHubContext { get; }
    }

    public class HubContextProvider : IHubContextProvider
    {
        public IHubContext<LobbyHub> LobbyHubContext { get; }

        public HubContextProvider(IHubContext<LobbyHub> lobbyHubContext)
        {
            LobbyHubContext = lobbyHubContext;
        }
    }
}