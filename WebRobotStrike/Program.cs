using BlazorApp1.Components;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;
using BlazorApp1.UsersServices;

var builder = WebApplication.CreateBuilder(args);

// Ajout UsersSerice
builder.Services.AddScoped<UsersService>();

// bdd
builder.Services.AddDbContext<RobostrikeContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Ajout des services pour Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();


app.Use(async (context, next) =>
{
    var usersService = context.RequestServices.GetRequiredService<UsersService>();

    var isAuthenticated = usersService.IsUserAuthenticated();

    if (!isAuthenticated && context.Request.Path.StartsWithSegments("/"))
    {
        usersService.SetUserSession(147852369);


        context.Response.Redirect("/login");
        return;
    }

    await next.Invoke();
});





if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();