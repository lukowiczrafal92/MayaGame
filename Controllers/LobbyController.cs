using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BoardGameBackend.Models;
using BoardGameBackend.Managers;
using BoardGameBackend.Hubs;
using BoardGameBackend.MiddleWare;
using AutoMapper;
using BoardGameBackend.Repositories;

namespace BoardGameBackend.Controllers
{
    [ApiController]
    [Route("api/lobby")]
    public class LobbyController : ControllerBase
    {
        private readonly IHubContext<LobbyHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly IGameBackupSaver _gameBackupSaver;
        public LobbyController(IHubContext<LobbyHub> hubContext, IMapper mapper, IGameBackupSaver gameBackupSaver)
        {
            _mapper = mapper;
            _hubContext = hubContext;
            _gameBackupSaver = gameBackupSaver;
        }

        // POST: api/lobby/create
        [Authorize]
        [HttpPost("create")]
        public IActionResult CreateLobby([FromBody] CreateLobbyDto createLobby)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                
                var userLobbies = LobbyManager.GetAllUserLobbies(user.Id);

                if(userLobbies.Count>0){
                    return BadRequest(new { Error = "You are already in a lobby" });
                }

                var lobby = LobbyManager.CreateLobby(user, createLobby);

                return Ok(new { lobbyId = lobby.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }

        }

        [Authorize]
        [HttpPost("join/{id}")]
        public IActionResult JoinLobby(string id)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                
                var userLobbies = LobbyManager.GetAllUserLobbies(user.Id);
                var playerIsInThatLobby = userLobbies.Any(l => l.Lobby.Id == id);
                if(playerIsInThatLobby){
                    return Ok(new { lobbyId = id, players = new List<PlayerInLobby> {} });
                }

                if(userLobbies.Count>0 && !playerIsInThatLobby){
                    return BadRequest(new { Error = "You are already in a lobby" });
                }

                var result = LobbyManager.JoinLobby(id, user);

                if (!result.Success)
                {
                    return BadRequest(new { Error = result.ErrorMessage });
                }

                _hubContext.Clients.Group(result.Lobby!.Id).SendAsync("PlayerJoined", _mapper.Map<Player>(user));
                return Ok(new { lobbyId = result.Lobby.Id, players = result.Lobby.Players });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error." });
            }
        }

        // POST: api/lobby/leave
        [Authorize]
        [HttpDelete("leave/{id}")]
        public async Task<IActionResult> LeaveLobby(string id)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var lobbyInfo = LobbyManager.LeaveLobby(id, user.Id);
                if (lobbyInfo == null)
                {
                    
                    await _hubContext.Clients.Group(id).SendAsync("LobbyDestroyed");
                    _gameBackupSaver.DeleteLobbyId(id);
                    return Ok("Lobby destroyed or player left.");
                }
                else
                {
                    await _hubContext.Clients.Group(id).SendAsync("PlayerLeft", _mapper.Map<Player>(user));
                    return Ok(new { lobbyId = lobbyInfo.Lobby.Id, players = lobbyInfo.Lobby.Players });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }

        [Authorize]
        [HttpPost("message/{id}")]
        public async Task<IActionResult> SendLobbyMessage(string id, [FromBody] SendLobbyMessageDto sendMessageDto)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                await _hubContext.Clients.Group(id).SendAsync("ReceiveMessage", _mapper.Map<UserModelDto>(user), sendMessageDto.Message);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }

        }

        [Authorize]
        [HttpDelete("destroy/{id}")]
        public async Task<IActionResult> DestroyLobby(string id)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var canDestroyLobby = LobbyManager.CanDestroyLobby(id, user);
                if (canDestroyLobby)
                {
                    await _hubContext.Clients.Group(id).SendAsync("DestroyLobby", _mapper.Map<Player>(user));
                    LobbyManager.DestroyLobby(id);
                    return Ok("Lobby destroyed successfully.");
                }
                return Unauthorized("Only the host can destroy the lobby.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }

        }

        [HttpGet("all")]
        public IActionResult GetAllLobbies()
        {
            try
            {
                var lobbies = LobbyManager.GetAllLobbies();

                return Ok(lobbies);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }

        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetLobbyById(string id)
        {
            try
            {
                var lobby = LobbyManager.GetLobbyById(id);
                if (lobby == null)
                {
                    return NotFound($"Lobby with ID {id} not found.");
                }
                return Ok(lobby);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }

        }

        [Authorize]
        [HttpPatch("updateInfo/{id}")]
        public async Task<IActionResult> UpdateLobbyInformation(string id, [FromBody] StartGameModel startGameModel)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var infoChanged = LobbyManager.ChangeLobbyInfo(id, startGameModel, user.Id);
                if (infoChanged == false)
                {
                    return NotFound();
                }
                await _hubContext.Clients.Group(id).SendAsync("UpdateLobbyInfo", startGameModel);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }

        [Authorize]
        [HttpGet("myLobbies")]
        public IActionResult GetUserLobbies()
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;

                var userLobbies = LobbyManager.GetAllUserLobbies(user.Id);

                return Ok(userLobbies);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }
    }
}