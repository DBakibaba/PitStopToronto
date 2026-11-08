using PitStop.API.Models;
using PitStop.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PitStop.API.Dtos;
namespace PitStop.API.Endpoints;

public static class WashroomEndpoints
{

    public static void MapWashroomEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/washrooms");

        group.MapGet("/", async (WashroomService washroomService) =>
        {

            var washrooms = await washroomService.GetAllWashroomsAsync();
            return Results.Ok(new
            {
                Count = washrooms.Count,
                Washrooms = washrooms
            });

        });



        group.MapGet("/nearby", async (double latitude, double longitude, LocationType? type, WashroomService washroomService) =>
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
                 longitude,
                 type
            );

            return Results.Ok(nearbyWashrooms);
        }).RequireRateLimiting("nearby");

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

            group.MapPost("/import-library", async (TorontoLibraryService torontoLibraryService) =>
            {
                await torontoLibraryService.GetTorontoLibrariesAsync();

                return Results.Ok();

            });




            group.MapPost("/import-osm-supermarket", async (OpenStreetMapService service) =>
            {
                var result = await service.GetSupermarketsLocationsAsync();

                return Results.Ok(new
                {
                    imported = result.Imported,
                    updated = result.Updated
                });
            });

            group.MapPost("/import-osm-home-depot", async (OpenStreetMapService service) =>
            {
                var result = await service.GetHomeDepotLocationAsync();

                return Results.Ok(new
                {
                    imported = result.Imported,
                    updated = result.Updated
                });
            });


            group.MapPost("/import-osm-canadianTire", async (OpenStreetMapService service) =>
            {
                var result = await service.GetCanadianTireLocationsAsync();

                return Results.Ok(new
                {
                    imported = result.Imported,
                    updated = result.Updated
                });
            });

            group.MapPost("/import-osm-tim-hortons", async (OpenStreetMapService service) =>
            {
                var result = await service.GetTimHortonsLocationsAsync();

                return Results.Ok(new
                {
                    imported = result.Imported,
                    updated = result.Updated
                });
            });


            group.MapPost("/import-osm-mcdonalds", async (OpenStreetMapService service) =>
            {
                var result = await service.GetMcDonaldsLocationsAsync();

                return Results.Ok(new
                {
                    imported = result.Imported,
                    updated = result.Updated
                });
            });

            group.MapPost("/import-osm-starbucks", async (OpenStreetMapService service) =>
            {
                var result = await service.GetStarbucksLocationsAsync();

                return Results.Ok(new
                {
                    imported = result.Imported,
                    updated = result.Updated
                });
            });


            group.MapPost("/import-osm-gasStation", async (OpenStreetMapService service) =>
            {
                var result = await service.GetGasStationsAsync();

                return Results.Ok(new
                {
                    imported = result.Imported,
                    updated = result.Updated
                });
            });
        }


        group.MapPost("/portable", async (AddPortableToiletRequest request,
            [FromHeader(Name = "X-Admin-Key")] string? adminKey,
            IConfiguration configuration, WashroomService washroomService) =>
        {
            string? expectedKey = configuration["AdminApiKey"];

            if(string.IsNullOrEmpty(expectedKey) || adminKey != expectedKey)
            {
                return Results.Unauthorized();
            }

            int newId = await washroomService.AddPortableToiletAsync(request.Latitude, request.Longitude);
            
            return Results.Created($"/washrooms/{newId}", new { id = newId });
        });

        group.MapGet("/waiting", async (
            [FromHeader(Name = "X-Admin-Key")] string? adminKey,
            IConfiguration configuration,
            WashroomService washroomService) =>
        {
            string? expectedKey = configuration["AdminApiKey"];

            if (string.IsNullOrEmpty(expectedKey) || adminKey != expectedKey)
            {
                return Results.Unauthorized();
            }

            var waiting = await washroomService.GetWaitingToiletsAsync();
            return Results.Ok(waiting);
        });
        group.MapPost("/{id:int}/approve", async (
            int id,
            [FromHeader(Name = "X-Admin-Key")] string? adminKey,
            IConfiguration configuration,
            WashroomService washroomService) =>
        {
            if (!IsAdmin(adminKey, configuration))
            {
                return Results.Unauthorized();
            }

            bool approved = await washroomService.ApproveToiletAsync(id);
            return approved ? Results.Ok() : Results.NotFound();
        });

        group.MapPost("/{id:int}/reject", async (
            int id,
            [FromHeader(Name = "X-Admin-Key")] string? adminKey,
            IConfiguration configuration,
            WashroomService washroomService) =>
        {
            if (!IsAdmin(adminKey, configuration))
            {
                return Results.Unauthorized();
            }

            bool rejected = await washroomService.RejectToiletAsync(id);
            return rejected ? Results.Ok() : Results.NotFound();
        });

    }

    private static bool IsAdmin(string? adminKey, IConfiguration configuration)
    {
        string? expectedKey = configuration["AdminApiKey"];
        return !string.IsNullOrEmpty(expectedKey) && adminKey == expectedKey;
    }
}