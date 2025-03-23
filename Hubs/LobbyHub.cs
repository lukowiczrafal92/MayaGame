using System.Collections.Concurrent;
using BoardGameBackend.Managers;
using BoardGameBackend.Models;
using Microsoft.AspNetCore.SignalR;
using BoardGameBackend.Repositories;
using AutoMapper;

namespace BoardGameBackend.Hubs
{
    public class LobbyHub : Hub
    {
        public static readonly ConcurrentDictionary<string, PlayerInLobby> ConnectionMappings = new ConcurrentDictionary<string, PlayerInLobby>();
        private static readonly ConcurrentDictionary<string, LobbyInfo> Lobbies = new ConcurrentDictionary<string, LobbyInfo>();
        private IAuthService _userService { get; set; }
        private IGameBackupSaver _backupService { get; set; }
        private readonly IMapper _mapper;

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async void AddBackupLobbies()
        {   
            var hList = await _backupService.GetAllBackupData();
            if(hList == null)
                return;

            foreach(var fmb in hList)
            {
                LobbyInfo li = new LobbyInfo(){
                    HostId = fmb.lobbyinfo.Lobby.HostId,
                    GameHasStarted = true
                };

                foreach(var p in fmb.lobbyinfo.Lobby.Players)
                    li.Players.Add(p.Id);

                Lobbies[fmb.lobbyinfo.Lobby.Id] = li;
            }
        }

        public LobbyHub(IAuthService userService, IGameBackupSaver backupService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
            _backupService = backupService;
            AddBackupLobbies();
        }

        public async Task MarkPlayerDisconnected(string lobbyId, PlayerInLobby player)
        {
            if (Lobbies.TryGetValue(lobbyId, out var lobbyInfo))
            {
                // Mark the player as disconnected instead of removing them if the game has started
                var disconnectedPlayer = lobbyInfo.Players.FirstOrDefault(p => p == player.Id);
                if (disconnectedPlayer != null)
                {
                    player.IsConnected = false;
                    await Clients.Group(lobbyId).SendAsync("PlayerDisconnected", player);
                }
            }
        }

        public async Task Disconnect()
        {
            // Check if the connection ID is mapped to a player
            if (ConnectionMappings.TryRemove(Context.ConnectionId, out var player))
            {
                // Get the lobby ID for the player
                var lobbyId = GetLobbyIdForPlayer(player.Id);

                if (lobbyId != null && Lobbies.TryGetValue(lobbyId, out var lobbyInfo))
                {
                    var fullLobbyInfo = LobbyManager.GetLobbyById(lobbyId);
                    if (fullLobbyInfo?.Lobby.GameId != null)
                    {

                        player.IsConnected = false;
                        await Clients.Group(lobbyId).SendAsync("PlayerDisconnected", player);
                    }
                    else
                    {
                        if (player.Id == lobbyInfo.HostId)
                        {
                            // If the player is the host and the game hasn't started, destroy the lobby
                            LobbyManager.DestroyLobby(lobbyId);
                            await LobbyDestroyed(lobbyId, player);
                        }
                        else
                        {
                            lobbyInfo.Players.Remove(player.Id);
                            LobbyManager.LeaveLobby(lobbyId, player.Id);
                            await Clients.Group(lobbyId).SendAsync("PlayerLeft", player);
                        }

                    }


                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectionMappings.TryRemove(Context.ConnectionId, out var disconnectedPlayer))
            {
                var lobbyId = GetLobbyIdForPlayer(disconnectedPlayer.Id);
                if (lobbyId != null && Lobbies.TryGetValue(lobbyId, out var lobbyInfo))
                {

                    var fullLobbyInfo = LobbyManager.GetLobbyById(lobbyId);
                    if (fullLobbyInfo?.Lobby.GameId != null)
                    {
                        await MarkPlayerDisconnected(lobbyId, disconnectedPlayer);
                    }
                    else
                    {
                        if (disconnectedPlayer.Id == lobbyInfo.HostId)
                        {
                            LobbyManager.DestroyLobby(lobbyId);
                            await LobbyDestroyed(lobbyId, disconnectedPlayer);
                        }
                        else
                        {
                            LobbyManager.LeaveLobby(lobbyId, disconnectedPlayer.Id);
                            lobbyInfo.Players.Remove(disconnectedPlayer.Id);
                            await Clients.Group(lobbyId).SendAsync("PlayerLeft", disconnectedPlayer);
                        }
                    }

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinLobby(string lobbyId)
        {
            var userIdClaim = Context.User?.FindFirst("id");
            var user = await _userService.GetUserById(Guid.Parse(userIdClaim!.Value));
            PlayerInLobby player = _mapper.Map<PlayerInLobby>(user);

            Console.WriteLine("Join lobby " + lobbyId);
            if (Lobbies.TryGetValue(lobbyId, out var lobbyInfo))
            {
                if (lobbyInfo.Players.Contains(player.Id))
                {
                Console.WriteLine("Seems okay " + lobbyId);
                    player.IsConnected = true;
                    var lobby = LobbyManager.GetLobbyById(lobbyId);
                    if (lobby?.Lobby.GameId != null)
                    {
                        var info = GameManager.GetGameData(lobby.Lobby.GameId, player.Id);
                        await Clients.Caller.SendAsync("PlayerRejoinedData", info);
                    }

                    await Clients.Group(lobbyId).SendAsync("PlayerRejoined", player);
                }
                else
                {
                Console.WriteLine("Does not contain player " + lobbyId);
                    lobbyInfo.Players.Add(player.Id);
                }
            }
            else
            {
                Console.WriteLine("Created new lobby " + lobbyId);
                lobbyInfo = new LobbyInfo
                {
                    Players = new List<Guid> { player.Id },
                    HostId = player.Id,
                    GameHasStarted = false
                };
                Lobbies[lobbyId] = lobbyInfo;
            }

            ConnectionMappings[Context.ConnectionId] = player;
            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
            await Clients.Group(lobbyId).SendAsync("PlayerJoined", new { player });
        }

        public async Task Reconnect(string lobbyId)
        {
                Console.WriteLine("Reconnect firing");
            var userIdClaim = Context.User?.FindFirst("id");
            var user = await _userService.GetUserById(Guid.Parse(userIdClaim!.Value));
            var player = _mapper.Map<PlayerInLobby>(user);

            if (Lobbies.TryGetValue(lobbyId, out var lobbyInfo))
            {
                if (lobbyInfo.Players.Contains(player.Id))
                {
                    player.IsConnected = true;
                    ConnectionMappings[Context.ConnectionId] = player;

                    await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

                    var lobby = LobbyManager.GetLobbyById(lobbyId);
                    if (lobby?.Lobby.GameId != null)
                    {
                        var gameData = GameManager.GetGameData(lobby.Lobby.GameId, player.Id);
                        await Clients.Caller.SendAsync("PlayerRejoinedData", gameData);
                    }

                    await Clients.Group(lobbyId).SendAsync("PlayerReconnected", player);
                }
                else
                {
                    lobbyInfo.Players.Add(player.Id);
                    ConnectionMappings[Context.ConnectionId] = player;
                    await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);
                    await Clients.Group(lobbyId).SendAsync("PlayerJoined", new { player });
                }
            }
            else
            {
                await Clients.Caller.SendAsync("ReconnectFailed", "Lobby not found or no longer exists.");
            }
        }

        private string? GetLobbyIdForPlayer(Guid playerId)
        {
            foreach (var kvp in Lobbies)
            {
                if (kvp.Value.Players.Contains(playerId))
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public async Task LobbyDestroyed(string lobbyId, PlayerInLobby player)
        {
            if (Lobbies.TryRemove(lobbyId, out var lobbyInfo))
            {
                await Clients.Group(lobbyId).SendAsync("DestroyLobby", player);
            }
            Console.WriteLine("Tutaj odpala się.");
            _backupService.DeleteLobbyId(lobbyId);
        }

    }
}