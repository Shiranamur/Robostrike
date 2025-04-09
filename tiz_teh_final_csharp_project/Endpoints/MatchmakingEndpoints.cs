// Endpoints/MatchmakingEndpoints.cs
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
        app.MapGet("/api/matchmaking/join", async (HttpRequest request, QueueManager queueManager) =>
            {
                var context = request.HttpContext;
                if (context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is int userId)
                {
                    queueManager.EnqueuePlayer(userId);
                    return Results.Ok(new { Message = "Queued successfully" });
                }
                return Results.Unauthorized();
            })
        .WithName("GetMatchmaking")
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
