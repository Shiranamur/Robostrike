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
    Console.WriteLine("PUT /api/game/{gameId}/inputs called");
    Console.WriteLine($"Received gameId: {gameId}");

    if (string.IsNullOrEmpty(gameId))
    {
        Console.WriteLine("BadRequest: gameId is null or empty");
        return Results.BadRequest("Must give a valid Game ID");
    }

    Game? game = gameManager.GetGame(gameId);
    Console.WriteLine(game == null ? "Game not found" : "Game found");

    if (game == null)
    {
        return Results.NotFound();
    }

    var context = request.HttpContext;

    if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userIdStr && int.TryParse(userIdStr, out var userId))
    {
        try
        {
            using var reader = new StreamReader(request.Body);
            string body = await reader.ReadToEndAsync();

            Console.WriteLine($"Raw body: {body}");

            string? playerMoves = JsonSerializer.Deserialize<string>(body);
            if (string.IsNullOrWhiteSpace(playerMoves))
            {
                Console.WriteLine("Deserialized playerMoves is null or empty");
                return Results.BadRequest("PlayerMoves missing or empty in body");
            }

            Console.WriteLine($"Extracted PlayerMoves: {playerMoves}");

            string playerMoves6Maxlength = new string(playerMoves.Take(6).ToArray());
            game.SubmitPlayerInput(userId, playerMoves6Maxlength);

            Console.WriteLine("SubmitPlayerInput called successfully");

            return Results.Ok("Moves Submitted");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception caught while processing input: {ex.Message}");
            return Results.InternalServerError("Unexpected behavior from the game occurred");
        }
    }

    Console.WriteLine("Invalid or missing UserId in context");
    return Results.BadRequest("Invalid user ID or missing PlayerMoves");
});

        app.MapGet("/api/game/{gameId}/status", async (string gameId, GameManager gameManager) =>
        {
            Console.WriteLine("GET /api/game/{gameId}/status called");
            Console.WriteLine($"Received gameId: {gameId}");

            if (string.IsNullOrEmpty(gameId))
            {
                Console.WriteLine("BadRequest: gameId is null or empty");
                return Results.BadRequest("Must give a valid Game ID");
            }

            Game? game = gameManager.GetGame(gameId);
            Console.WriteLine(game == null ? "Game not found" : "Game found");

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
