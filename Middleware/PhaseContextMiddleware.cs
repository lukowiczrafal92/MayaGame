using BoardGameBackend.Managers;
using BoardGameBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace BoardGameBackend.MiddleWare
{
    public class PhaseCheckFilterAttribute : ActionFilterAttribute
    {
        private readonly PhaseType _requiredPhaseType;

        public PhaseCheckFilterAttribute(PhaseType requiredPhaseType)
        {
            if (requiredPhaseType == null)
            {
                throw new ArgumentException("Invalid phase type provided.");
            }
            _requiredPhaseType = requiredPhaseType;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = (UserModel)context.HttpContext.Items["User"]!;
            var routeValues = context.ActionArguments;
            if (routeValues.TryGetValue("id", out var lobbyIdValue) && lobbyIdValue is string lobbyId)
            {
                var lobbyInfo = LobbyManager.GetLobbyById(lobbyId);
                var lobby = lobbyInfo?.Lobby;

                if (lobby == null || string.IsNullOrEmpty(lobby.GameId))
                {
                    context.Result = new NotFoundObjectResult("Lobby or game not found.");
                    return;
                }

                var gameContext = GameManager.GetGameById(lobby.GameId);

                if (gameContext == null)
                {
                    context.Result = new NotFoundObjectResult("Game not found.");
                    return;
                }
                var cPlayer = gameContext.PlayerManager.GetPlayerById(user.Id);
                if (cPlayer == null || !cPlayer.Active)
                {
                    context.Result = new ForbidResult("It's not your turn.");
                    return;
                }

                var currentPhase = gameContext.PhaseManager.GetCurrentPhase();

                if (currentPhase == null || currentPhase.PhaseType != _requiredPhaseType)
                {
                    context.Result = new BadRequestObjectResult($"Invalid phase. Current phase is null or different, but {_requiredPhaseType} is required.");
                    return;
                }

                context.HttpContext.Items["Player"] = cPlayer;
                context.HttpContext.Items["Lobby"] = lobby;
                context.HttpContext.Items["GameContext"] = gameContext;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}