// Endpoints/MatchmakingEndpoints.cs

using System.Reflection.Metadata;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json;
using tiz_teh_final_csharp_project.Extensions;

namespace tiz_teh_final_csharp_project.Endpoints;

public class MatchmakingEndpoints : IEndpointMapper
{
    public void Endpoints(WebApplication app)
    {
        app.MapGet("/api/matchmaking/join", (HttpRequest request, QueueManager queueManager) =>
            {
                var context = request.HttpContext;
                if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userIdStr && int.TryParse(userIdStr, out var userId))
                {
                    if (queueManager.EnqueuePlayer(userId)) // returns true if player added to the queue successfully
                    {
                        Console.WriteLine($"[Debug] Player {userId} joined");
                        return Results.Ok(new { Message = "Queued successfully" });
                    }
                    
                    Console.WriteLine($"[Debug] Player hasn't {userId} joined");
                    return Results.Conflict(new { Message = "Failed to enqueue player" });

                }
                return Results.Unauthorized();
            })
        .WithName("GetMatchmaking")
        .Produces(200)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi(ConfigureOpenApiOperation);
        

        app.MapGet("/api/matchmaking/leave", (HttpRequest request, QueueManager queueManager) =>
        {
            var context = request.HttpContext;
            if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userIdStr &&
                int.TryParse(userIdStr, out var userId))
            {
                queueManager.DequeuePlayer(userId);
                return Results.Ok(new { Message = "Dequeued successfully" });
            }

            return Results.Unauthorized();
        })
        .WithName("LeaveMatchmaking")
        .Produces(200)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi(ConfigureOpenApiOperation);

        
        app.MapGet("api/matchmaking/status", async (HttpRequest request, QueueManager queueManager, GameManager gameManager) =>
        {
            var context = request.HttpContext;

            // user has valid id and is queued for matchmaking
            if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userIdStr &&
                int.TryParse(userIdStr, out var userId) && ( queueManager.GetQueuedPlayerIds().Contains(userId) || gameManager.IsInPendingNotifications(userId)))
            {
                var tcs = new TaskCompletionSource<object>();
                var cancellationToken = context.RequestAborted;

                // Register cancellation so if the client disconnects, the waiting task is canceled.
                cancellationToken.Register(() => tcs.TrySetCanceled());

                // Register a callback for the specific player.
                // When QueueManager.UpdateStatus is called, this callback triggers.
                gameManager.OnStatusUpdate(userId, status =>
                {
                    Console.WriteLine($"[Debug] Update callback triggered for user Matchmaking API {userId} with status: {status}");
                    tcs.TrySetResult(new { Status = status });
                });


                try
                {
                    // Wait for the status update or timeout
                    var result = await Task.WhenAny(tcs.Task, Task.Delay(30000, cancellationToken));
                    if (result == tcs.Task)
                    {
                        // expects to return ["game_id": "<matchId>"] the value of matchId will be returned in a string
                        Console.WriteLine($"[Debug] Player {userId} joined game {tcs.Task.Result}");
                        return Results.Ok(tcs.Task.Result);
                    }
                    else
                    {
                        queueManager.DequeuePlayer(userId); // dequeue after delay passed
                        Console.WriteLine($"[Debug] Player {userId} is timed out");
                        return Results.Ok(new { Status = "No updates" });
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"[Debug] Player {userId} left the matchmaking");
                    return Results.Ok(new { Status = "Request canceled" });
                }
            }
            Console.WriteLine($"[debug] User not in the matchmaking list");
            return Results.Unauthorized();
        })
        .WithName("MatchmakingStatus")
        .Produces(200)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi(ConfigureOpenApiOperation);

    }

    private OpenApiOperation ConfigureOpenApiOperation(OpenApiOperation operation)
    {
        operation.Summary = "Enter Matchmaking";
        operation.Description = "Joins a player into matchmaking using the token provided in the HTTP context.";
        operation.Responses["200"] = new OpenApiResponse 
        {
            Description = "Player successfully joined matchmaking",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["Message"] = new OpenApiString("Queued successfully")
                    }
                }
            }
        };
        operation.Responses["401"] = new OpenApiResponse 
        {
            Description = "Unauthorized - UserId missing or invalid"
        };
        return operation;
    }
    
    
    
}
