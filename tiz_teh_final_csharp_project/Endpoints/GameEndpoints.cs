// Endpoints/MapEndpoints.cs
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json;
using tiz_teh_final_csharp_project.Extensions;
using tiz_teh_final_csharp_project.Endpoints.Class;

namespace tiz_teh_final_csharp_project.Endpoints;

public class GameEndpoints : IEndpointMapper
{
    public void Endpoints(WebApplication app)
    {
        app.MapPut("/api/game/{gameId}/inputs", async (HttpRequest request, string gameId, GameManager gameManager) =>
            {
                if (string.IsNullOrEmpty(gameId))
                {
                    return Results.BadRequest("Must give a valid Game ID");
                }
                Game? game = gameManager.GetGame(gameId);

                if (game == null)
                {
                    return Results.NotFound();
                }

                var context = request.HttpContext;

                if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userIdStr &&
                    int.TryParse(userIdStr, out var userId) && context.Items.TryGetValue("PlayerMoves", out var playerMovesObj) && playerMovesObj is string playerMoves)
                {
                    try
                    {
                        string playerMoves6Maxlength = new string(playerMoves.Take(6).ToArray());
                        game.SubmitPlayerInput(userId, playerMoves6Maxlength);
                        Console.WriteLine("test");
                        return Results.Ok("Moves Submitted");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        return Results.InternalServerError("Unexpected behavior from the game occurred");
                    }
                }
    
                return Results.BadRequest("Invalid user ID or player moves");
            })
            .WithName("Send input")
            .Produces<MapResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/game/{gameId}/status", async (string gameId, GameManager gameManager) =>
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return Results.BadRequest("Must give a valid Game ID");
            }
            Game? game = gameManager.GetGame(gameId);

            if (game == null)
            {
                return Results.NotFound();
            }

            try
            { 
                // returns the gamestate
                return Results.Ok(new { Status = new { Round = game.GetCurrentRoundState() } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Results.InternalServerError("Unexpected behavior from the game occurred");
            }

            return Results.Ok(new { Status = new { game_id = gameId } });
        });
    }
}
