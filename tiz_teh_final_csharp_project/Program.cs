using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tiz_teh_final_csharp_project.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapEndpoints();

await app.RunAsync();