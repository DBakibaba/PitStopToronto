using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using PitStop.API.Data;
using PitStop.API.Endpoints;
using PitStop.API.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;



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
builder.Services.AddHttpClient<OpenStreetMapService>(client=>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("PitStop/1.0 (github.com/DBakibaba)");
});
builder.Services.AddScoped<OperatingHoursParser>();
string frontendOrigin = builder.Configuration["FrontendOrigin"]
    ?? throw new InvalidOperationException("FrontendOrigin is not configured.");
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(frontendOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("nearby", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();
app.UseCors("Frontend");
app.UseRateLimiter();
app.MapWashroomEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}




app.Run();
