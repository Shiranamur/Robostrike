using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using tiz_teh_final_csharp_project.Repositories;

namespace tiz_teh_final_csharp_project.Endpoints;

public class Middleware
{
    private readonly RequestDelegate _next;
    private readonly IUserRepository _userRepository;

    public Middleware(RequestDelegate next, IUserRepository userRepository)
    {
        _next = next;
        _userRepository = userRepository;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // before the NEXT api call
        Console.WriteLine("Before api call");
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) &&
            authHeader.ToString().StartsWith("Bearer "))
        {
            Console.WriteLine("Bearer found");
            string token = authHeader.ToString().Substring("Bearer ".Length).Trim();
            var userId = await _userRepository.GetUserIdByTokenAsync(token);
            if (userId != null)
            {
                context.Items["UserId"] = userId;
            }
            else
            {
                Console.WriteLine("Bearer not found");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Invalid token");
            }


            await _next(context);
        }
        else
        {
            Console.WriteLine("No token");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
        }
    }
}