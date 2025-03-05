using BoardGameBackend.Managers;
using BoardGameBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace BoardGameBackend.MiddleWare
{
    public class MiniPhaseCheckFilterAttribute : ActionFilterAttribute
    {
        private readonly Type _requiredMiniPhaseType;

    /*    public MiniPhaseCheckFilterAttribute(Type requiredMiniPhaseType)
        {
            if (requiredMiniPhaseType == null || !typeof(MiniPhase).IsAssignableFrom(requiredMiniPhaseType))
            {
                throw new ArgumentException("Invalid phase type provided.");
            }
            _requiredMiniPhaseType = requiredMiniPhaseType;
        } */

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

             /*   var currentPlayer = gameContext!.TurnManager.CurrentPlayer;
                if (currentPlayer== null || currentPlayer?.Id != user.Id)
                {
                    context.Result = new ForbidResult("It's not your turn.");
                }

                var currentMiniPhase = gameContext.MiniPhaseManager.CurrentMiniPhase!;

                if (currentMiniPhase.GetType() != _requiredMiniPhaseType)
                {
                    context.Result = new BadRequestObjectResult($"Invalid phase. Current phase is {currentMiniPhase.Name}, but {((Phase)Activator.CreateInstance(_requiredMiniPhaseType))!.Name} is required.");
                    return;
                } */

            //    context.HttpContext.Items["Player"] = currentPlayer;
                context.HttpContext.Items["Lobby"] = lobby;
                context.HttpContext.Items["GameContext"] = gameContext;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}