using BoardGameBackend.Models;

namespace BoardGameBackend.Managers;


public static class LobbyManager
{
    private static readonly List<LobbyManagerInfo> Lobbies = new List<LobbyManagerInfo>();
    public static readonly int MAX_AMOUNT_OF_USERS_IN_LOBBY = 6;
    private static readonly object _lock = new object();

    public static Lobby CreateLobby(UserModel user, CreateLobbyDto createLobbyDto)
    {
        lock (_lock)
        {
            var player = new PlayerInLobby { Id = user.Id, Name = user.Username, IsConnected = false };
            var lobby = new Lobby { HostId = user.Id, LobbyName = createLobbyDto.LobbyName };
            lobby.Players.Add(player);
            Lobbies.Add(new LobbyManagerInfo { Lobby = lobby });
            return lobby;
        }
    }

    public static bool ChangeLobbyInfo(string lobbyId, StartGameModel startGameModel, Guid userId)
    {
        var lobbyInfo = GetLobbyById(lobbyId);
        if (lobbyInfo != null && lobbyInfo.Lobby.HostId == userId)
        {
            lobbyInfo.StartGameModel = startGameModel;
            return true;
        }

        return false;
    }

    public static void SetGameIdForLobby(string lobbyId, string gameId)
    {
        var lobbyInfo = GetLobbyById(lobbyId);
        if (lobbyInfo != null)
        {
            lobbyInfo.Lobby.GameId = gameId;
        }
    }


    public static LobbyJoinResult JoinLobby(string lobbyId, UserModel user)
    {
        var lobbyInfo = Lobbies.FirstOrDefault(l => l.Lobby.Id == lobbyId);
        if (lobbyInfo == null)
        {
            return new LobbyJoinResult { ErrorMessage = "Lobby not found." };
        }
        if (lobbyInfo.Lobby.Players.Any(p => p.Id == user.Id))
        {
            return new LobbyJoinResult { ErrorMessage = "User is already in the lobby." };
        }
        if (lobbyInfo.Lobby.Players.Count >= MAX_AMOUNT_OF_USERS_IN_LOBBY)
        {
            return new LobbyJoinResult { ErrorMessage = "Lobby is full." };
        }
        if (lobbyInfo.Lobby.GameId != null)
        {
            return new LobbyJoinResult { ErrorMessage = "Lobby is already started." };
        }



        var player = new PlayerInLobby { Id = user.Id, Name = user.Username, IsConnected = true };
        lobbyInfo.Lobby.Players.Add(player);

        return new LobbyJoinResult { Lobby = lobbyInfo.Lobby };
    }

    public static LobbyManagerInfo? LeaveLobby(string lobbyId, Guid id)
    {
        var lobbyInfo = Lobbies.FirstOrDefault(l => l.Lobby.Id == lobbyId);
        if (lobbyInfo != null)
        {
            var player = lobbyInfo.Lobby.Players.FirstOrDefault(p => p.Id == id);
            if (player != null)
            {
                lobbyInfo.Lobby.Players.Remove(player);

                if (lobbyInfo.Lobby.HostId == player.Id)
                {
                    Lobbies.Remove(lobbyInfo);
                    return null;
                }
            }
        }
        return lobbyInfo;
    }

    public static bool CanDestroyLobby(string lobbyId, UserModel user)
    {
        var lobbyInfo = Lobbies.FirstOrDefault(l => l.Lobby.Id == lobbyId);

    /*    if (lobbyInfo != null && lobbyInfo.Lobby.HostId == user.Id)
        {
            return true;
        } */
        return false;
    }

    public static void DestroyLobby(string lobbyId)
    {
        var lobbyInfo = Lobbies.FirstOrDefault(l => l.Lobby.Id == lobbyId);
        if (lobbyInfo != null)
        {
            Lobbies.Remove(lobbyInfo);
        }

    }

    public static LobbyManagerInfo? GetLobbyById(string lobbyId)
    {
        var lobbyInfo = Lobbies.FirstOrDefault(l => l.Lobby.Id == lobbyId);

        return lobbyInfo;
    }

    public static Lobby? GetLobbyByGameId(string gameId)
    {
        var lobbyInfo = Lobbies.FirstOrDefault(l => l.Lobby.GameId == gameId);

        return lobbyInfo?.Lobby;
    }

    public static List<LobbyManagerInfo> GetAllUserLobbies(Guid userId)
    {
        var userLobbies = Lobbies.Where(lobbyInfo => lobbyInfo.Lobby.Players.Any(player => player.Id == userId)).ToList();
        return userLobbies; 
    }

    public static GameContext? StartGame(string lobbyId, UserModel user)
    {
        var lobbyInfo = Lobbies.FirstOrDefault(l => l.Lobby.Id == lobbyId);
        if (lobbyInfo != null && lobbyInfo.Lobby.HostId == user.Id)
        {
            var gameContext = GameManager.StartGameFromLobby(lobbyInfo.Lobby, lobbyInfo.StartGameModel);
            SetGameIdForLobby(lobbyId, gameContext.GameId);
            return gameContext;
        }
        return null;
    }

    public static List<LobbyManagerInfo> GetAllLobbies()
    {
        return Lobbies; // Assuming Lobbies is a dictionary or similar collection
    }

}