using Microsoft.AspNetCore.Mvc;
using BoardGameBackend.Managers;
using BoardGameBackend.Models;
using BoardGameBackend.MiddleWare;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;
using BoardGameBackend.Hubs;

namespace BoardGameBackend.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<LobbyHub> _hubContext;
        private readonly IMapper _mapper;

        public GameController(IHubContext<LobbyHub> hubContext, IMapper mapper)
        {
            _hubContext = hubContext;
            _mapper = mapper;
        }

        [HttpGet("start/{id}")]
        [Authorize] // Ensure only authenticated users can start a game
        public async Task<ActionResult> StartGame(string id)
        {
            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var gameContext = LobbyManager.StartGame(id, user);
            if (gameContext == null)
            {
                return BadRequest("Failed to start the game. Ensure the lobby exists and the user is the host.");
            }
            
            gameContext.StartGame();
            
            return Ok(new { gameContext.GameId, Message = "Game started successfully.", gameContext.PlayerManager.Players });
        }
    }
}