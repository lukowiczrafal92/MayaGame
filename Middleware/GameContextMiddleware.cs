namespace BoardGameBackend.MiddleWare;

using BoardGameBackend.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class GameContextFilterAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
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

            context.HttpContext.Items["Lobby"] = lobby;
            context.HttpContext.Items["GameContext"] = gameContext;
        }
        
        await base.OnActionExecutionAsync(context, next);
    }
}