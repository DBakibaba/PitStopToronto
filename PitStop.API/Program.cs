using Microsoft.EntityFrameworkCore;
using PitStop.API.Data;
using PitStop.API.Endpoints;
using PitStop.API.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("PitStopDatabase");

builder.Services.AddDbContext<PitStopDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<WashroomService>();

builder.Services.AddHttpClient();

var app = builder.Build();
app.MapWashroomEndpoints();


app.Run();