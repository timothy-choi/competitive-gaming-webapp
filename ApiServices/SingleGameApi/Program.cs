using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dapper;
using CompetitiveGamingApp.TypeHandlers; // Add this for JsonTypeHandler
using CompetitiveGamingApp.Models;  
using CompetitiveGamingApp.TimeSpanHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDBService, DBService>();
builder.Services.AddSingleton<SingleGameServices>();
SqlMapper.AddTypeHandler(new JsonTypeHandler<InGameScore>());
SqlMapper.AddTypeHandler(new TimeSpanHandler());
SqlMapper.AddTypeHandler(new DictionaryHandler());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use routing
app.UseRouting();

// Map controllers to routes
app.MapControllers(); // This maps at

app.Run();
