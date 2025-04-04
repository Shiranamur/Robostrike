using BlazorApp1.Components;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;
using BlazorApp1.UsersServices;
using BlazorApp1.Services;

var builder = WebApplication.CreateBuilder(args);

// Ajout UsersSerice
builder.Services.AddScoped<UsersService>();

builder.Services.AddScoped<SessionService>();


// bdd
builder.Services.AddDbContext<RobostrikeContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));


// Ajout des services pour Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
    {
        var token = authHeader.Substring("Bearer ".Length).Trim();
        var sessionService = context.RequestServices.GetRequiredService<SessionService>();
        var user = await sessionService.ValidateSessionTokenAsync(token);
        if (user != null)
        {
            // Token is valid. Possibly store user in context.Items
            context.Items["CurrentUser"] = user;
        }
    }
    await next.Invoke();
});



app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();