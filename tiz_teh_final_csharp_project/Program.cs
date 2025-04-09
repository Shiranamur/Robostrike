using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tiz_teh_final_csharp_project.Endpoints;
using tiz_teh_final_csharp_project;
using tiz_teh_final_csharp_project.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<QueueManager>();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton<MapHelper>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the middleware
builder.Services.AddTransient<Middleware>();


var app = builder.Build();

// show environment type
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Dynamically add all endpoint mappings
app.Endpoints();

await app.RunAsync();