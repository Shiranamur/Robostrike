// Endpoints/MapEndpoints.cs
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json;
using api.Endpoints.Class;
using api.Extensions;

namespace api.Endpoints;

public class MapEndPoints : IEndpointMapper
{
    public void Endpoints(WebApplication app)
    {
        app.MapPut("/map", async (HttpRequest request) =>
        {
            try
            {
                // read json from request body
                using StreamReader reader = new(request.Body);
                string jsonContent = await reader.ReadToEndAsync();

                // deserialize the json to the map object
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                Map? map = JsonSerializer.Deserialize<Map>(jsonContent, options);

                if (map == null)
                    return Results.BadRequest("invalid map data");

                return Results.Ok(new MapResponse
                {
                    Message = "Map received successfully",
                    MapDetails = map
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing map: {ex.Message}");
                return Results.Problem($"Error processing map: {ex.Message}");
            }
        })
        .WithName("CreateMap")
        .Produces<MapResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi(ConfigureOpenApiOperation);
    }

    private OpenApiOperation ConfigureOpenApiOperation(OpenApiOperation operation)
    {
        operation.Summary = "Create or update a map";
        operation.Description = "Receives a map in JSON format, processes it and returns the processed map";
    
        if (operation.RequestBody == null)
        {
            operation.RequestBody = new OpenApiRequestBody();
        }
    
        var tilesArray = new OpenApiArray();
        for (int i = 0; i < 10; i++)
        {
            tilesArray.Add(new OpenApiObject {
                ["X"] = new OpenApiInteger(i),
                ["Y"] = new OpenApiInteger(i),
                ["Type"] = new OpenApiString("void")
            });
        }
    
        var example = new OpenApiExample {
            Summary = "Sample Map",
            Value = new OpenApiObject {
                ["Width"] = new OpenApiInteger(10),
                ["Height"] = new OpenApiInteger(10),
                ["tiles"] = tilesArray
            }
        };
    
        if (operation.RequestBody.Content == null)
        {
            operation.RequestBody.Content = new Dictionary<string, OpenApiMediaType>();
        }
    
        string key = "application/json";
        if (!operation.RequestBody.Content.ContainsKey(key))
        {
            operation.RequestBody.Content[key] = new OpenApiMediaType();
        }
    
        if (operation.RequestBody.Content[key].Examples == null)
        {
            operation.RequestBody.Content[key].Examples = new Dictionary<string, OpenApiExample>();
        }
    
        operation.RequestBody.Content[key].Examples.Add("sampleMap", example);
    
        return operation;
    } 
}
