using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using PitStop.API.Data;
using PitStop.API.Endpoints;
using PitStop.API.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var connectionString =
    builder.Configuration.GetConnectionString("PitStopDatabase");

builder.Services.AddDbContext<PitStopDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<WashroomService>();
builder.Services.AddScoped<TorontoWashroomService>();
builder.Services.AddScoped<TorontoLibraryService>();
builder.Services.AddHttpClient();
 

var app = builder.Build();
app.MapWashroomEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.Run();
