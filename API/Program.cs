using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using api;
using api.Endpoints;
using api.Extensions;
using api.Repositories;

var builder = WebApplication.CreateBuilder(args);

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

builder.Configuration
    .AddJsonFile(Path.Combine(projectRoot, "appsettings.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(
        Path.Combine(projectRoot, $"appsettings.{builder.Environment.EnvironmentName}.json"),
        optional: true, reloadOnChange: true
    )
    .AddEnvironmentVariables();

builder.Services.AddSingleton<QueueManager>();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton<MapHelper>();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
}


var connectionString = builder.Configuration.GetConnectionString("DBConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DBConnection' not found.");

builder.Services.AddTransient<IUserRepository>(_ => new UserRepository(connectionString));

var app = builder.Build();

app.Services.GetRequiredService<GameManager>();
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<Middleware>();
app.Endpoints();

await app.RunAsync();
