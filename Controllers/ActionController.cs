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
    [Route("api/action")]
    public class ActionController : ControllerBase
    {
        private readonly IHubContext<LobbyHub> _hubContext;
        private readonly IMapper _mapper;

        public ActionController(IHubContext<LobbyHub> hubContext, IMapper mapper)
        {
            _hubContext = hubContext;
            _mapper = mapper;
        }

        [HttpPost("HandCards/{id}")]
        [Authorize]
        [PhaseCheckFilter(PhaseType.ActionCardSelection)]
        public ActionResult HandCards(string id, [FromBody] ActionCardsPickModel data)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var gameContext = (GameContext)Request.HttpContext.Items["GameContext"]!;
                var player = (PlayerInGame)Request.HttpContext.Items["Player"]!;
           
                var actionValid = gameContext.ActionManager.ChooseActionCardsToHand(data.ActionCardsIds, player);
                if(actionValid == false){
                    return BadRequest(new { Error = "Unable to make action." });
                }

                return Ok(new { Message = "Action completed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }

        [HttpPost("ActionForm/{id}")]
        [Authorize]
        [PhaseCheckFilter(PhaseType.PlayerAction)]
        public ActionResult ActionForm(string id, [FromBody] ActionFormSend data)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var gameContext = (GameContext)Request.HttpContext.Items["GameContext"]!;
                var player = (PlayerInGame)Request.HttpContext.Items["Player"]!;
           
                var actionValid = gameContext.ActionManager.ReceivedActionForm(data, player);
                if(actionValid == false){
                    return BadRequest(new { Error = "Unable to make action." });
                }

                return Ok(new { Message = "Action completed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }

        
        [HttpPost("ConvertPlayerAction/{id}")]
        [Authorize]
        [PhaseCheckFilter(PhaseType.PlayerAction)]
        public ActionResult ConvertPlayerAction(string id, [FromBody] SingleIntModel data)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var gameContext = (GameContext)Request.HttpContext.Items["GameContext"]!;
                var player = (PlayerInGame)Request.HttpContext.Items["Player"]!;
           
                var actionValid = gameContext.ActionManager.ConvertResourceDuringPlayerAction(data.Value, player);
                if(actionValid == false){
                    return BadRequest(new { Error = "Unable to make action." });
                }

                return Ok(new { Message = "Action completed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }
        
        [HttpPost("ConvertEndGame/{id}")]
        [Authorize]
        [PhaseCheckFilter(PhaseType.EndScoreConverting)]
        public ActionResult ConvertEndGame(string id, [FromBody] SingleIntModel data)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var gameContext = (GameContext)Request.HttpContext.Items["GameContext"]!;
                var player = (PlayerInGame)Request.HttpContext.Items["Player"]!;
           
                var actionValid = gameContext.ActionManager.ConvertResourceDuringEndGame(data.Value, player);
                if(actionValid == false){
                    return BadRequest(new { Error = "Unable to make action." });
                }

                return Ok(new { Message = "Action completed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }

        [HttpPost("ConvertExtraPhase/{id}")]
        [Authorize]
        [PhaseCheckFilter(PhaseType.IncomeConverting)]
        public ActionResult ConvertExtraPhase(string id, [FromBody] SingleIntModel data)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var gameContext = (GameContext)Request.HttpContext.Items["GameContext"]!;
                var player = (PlayerInGame)Request.HttpContext.Items["Player"]!;
           
                var actionValid = gameContext.ActionManager.ConvertResourceDuringExtraPhase(data.Value, player);
                if(actionValid == false){
                    return BadRequest(new { Error = "Unable to make action." });
                }

                return Ok(new { Message = "Action completed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }
        
        [HttpPost("PassExtraConvert/{id}")]
        [Authorize]
        [PhaseCheckFilter(PhaseType.IncomeConverting)]
        public ActionResult PassExtraConvert(string id, [FromBody] SingleIntModel data)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var gameContext = (GameContext)Request.HttpContext.Items["GameContext"]!;
                var player = (PlayerInGame)Request.HttpContext.Items["Player"]!;
           
                var actionValid = gameContext.ActionManager.PassExtraConvertPhase(player);
                if(actionValid == false){
                    return BadRequest(new { Error = "Unable to make action." });
                }

                return Ok(new { Message = "Action completed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }

        [HttpPost("PassEndGameConvert/{id}")]
        [Authorize]
        [PhaseCheckFilter(PhaseType.EndScoreConverting)]
        public ActionResult PassEndGameConvert(string id, [FromBody] SingleIntModel data)
        {
            try
            {
                UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
                var gameContext = (GameContext)Request.HttpContext.Items["GameContext"]!;
                var player = (PlayerInGame)Request.HttpContext.Items["Player"]!;
           
                var actionValid = gameContext.ActionManager.PassEndGameConvertPhase(player);
                if(actionValid == false){
                    return BadRequest(new { Error = "Unable to make action." });
                }

                return Ok(new { Message = "Action completed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }
    }
}