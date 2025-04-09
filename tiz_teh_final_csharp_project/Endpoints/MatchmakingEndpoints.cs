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
        app.MapGet("/api/matchmaking/join", async (HttpRequest request) =>
        {
            string authHeader = request.Headers["Authorization"].ToString();
            string token = string.Empty;

            if (!string.IsNullOrEmpty(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
        })
        .WithName("GetMatchmaking")
        .Produces(200);
    }
}