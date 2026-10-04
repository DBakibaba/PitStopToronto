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
builder.Services.AddScoped<OperatingHoursParser>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();
app.UseCors("Frontend");
app.MapWashroomEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


var parser = new OperatingHoursParser();
var testHours ="Mon 9 a.m. to 8:30 p.m.; Tue 9 a.m. to 8:30 p.m.";
var hours=parser.Parse(testHours);

foreach (var hour in hours)
{
    Console.WriteLine(
        $"{hour.DayOfWeek}: {hour.OpenTime} - {hour.CloseTime}"
    );
}

app.Run();
