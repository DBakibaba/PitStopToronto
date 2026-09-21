using PitStop.API.Models;
using PitStop.API.Services;

using Microsoft.EntityFrameworkCore;
namespace PitStop.API.Endpoints;

public static class WashroomEndpoints
{

    public static void MapWashroomEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/washrooms");

        group.MapGet("/nearby", async (
            double latitude,
            double longitude,
            WashroomService washroomService) =>


 {
     if (latitude < -90 || latitude > 90)
     {
         return Results.BadRequest("Latitude must be between -90 and 90.");
     }
     if (longitude < -180 || longitude > 180)
     {
         return Results.BadRequest("Longitude must be between -180 and 180.");
     }

     var nearbyWashrooms = await washroomService.GetNearByAsync(
         latitude,
         longitude
     );

     return Results.Ok(nearbyWashrooms);
 });
        if (app.Environment.IsDevelopment())
        {
            group.MapPost("/import-toronto", async (TorontoWashroomService torontoWashroomService) =>
            {
                var result = await torontoWashroomService.GetTorontoWashroomsAsync();

                return Results.Ok(new
                {
                    imported = result.Imported,
                    updated = result.Updated
                });
            });
        }
    }
}