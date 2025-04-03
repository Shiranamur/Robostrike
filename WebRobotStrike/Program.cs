using BlazorApp1.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Ajout du service DbContext pour la base de données MySQL
builder.Services.AddDbContext<RobostrikeContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Ajout des services nécessaires pour l'authentification par cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirection vers la page de login si non authentifié
    });

// Ajout des services MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware pour l'authentification
app.UseAuthentication();
app.UseAuthorization();

// Configuration des routes
app.MapControllers();

app.Run();
