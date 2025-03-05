using Microsoft.AspNetCore.Mvc;
using BoardGameBackend.Managers;
using BoardGameBackend.Models;
using BoardGameBackend.Hubs;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;
using BoardGameBackend.MiddleWare;

namespace BoardGameBackend.Controllers
{
    [ApiController]
    [Route("api/turn")]
    public class TurnController : ControllerBase
    {
        private readonly IHubContext<LobbyHub> _hubContext;
        private readonly IMapper _mapper;

        public TurnController(IHubContext<LobbyHub> hubContext, IMapper mapper)
        {
             _hubContext = hubContext;
             _mapper = mapper;
        }

    /*    [HttpGet("end/{id}")]
        [Authorize] 
        [PhaseCheckFilter(typeof(DummyPhase))]
        public ActionResult EndTurn(string id)
        {
            try{
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var gameContext = HttpContext.Items["GameContext"] as GameContext;

                gameContext!.TurnManager.EndTurn();
                //await _hubContext.Clients.Group(id).SendAsync("TurnEnded", gameContext.PlayerManager.Players[gameContext.TurnManager.CurrentPlayerIndex], gameContext.TurnManager.TurnCount);
                return Ok(new { Message = "Turn ended successfully." });
            }        
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        } */
    }
}