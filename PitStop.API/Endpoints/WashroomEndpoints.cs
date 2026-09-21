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
     var nearbyWashrooms = await washroomService.GetNearByAsync(
         latitude,
         longitude
     );

     return Results.Ok(nearbyWashrooms);
 });

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