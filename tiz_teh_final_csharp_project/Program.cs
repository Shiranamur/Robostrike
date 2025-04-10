using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tiz_teh_final_csharp_project;
using tiz_teh_final_csharp_project.Endpoints;
using tiz_teh_final_csharp_project.Extensions;
using tiz_teh_final_csharp_project.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<QueueManager>();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton<MapHelper>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the middleware
/* var connectionString = builder.Configuration.GetConnectionString("MariaDBConnection");
if(string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'MariaDBConnection' not found.");
}*/
builder.Services.AddTransient<IUserRepository>(provider => new UserRepository("Server=localhost;Port=3306;Database=robotstrike;User=root;Password=;"));


var app = builder.Build();


// Force instantiation of GameManager so it can subscribe to events of QueueManager
app.Services.GetRequiredService<GameManager>();

// show environment type
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use the custom middleware without registering it in DI manually.
app.UseMiddleware<Middleware>();

// Dynamically add all endpoint mappings
app.Endpoints();

await app.RunAsync();