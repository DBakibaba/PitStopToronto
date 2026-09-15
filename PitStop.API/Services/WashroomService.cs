using Microsoft.EntityFrameworkCore;
using PitStop.API.Data;
using PitStop.API.Dtos;
using PitStop.API.Models;

namespace PitStop.API.Services;

public class WashroomService(PitStopDbContext dbContext)
{
    public async Task<List<Washroom>> GetAllWashroomsAsync()
    {

        return await dbContext.Washrooms.ToListAsync();

    }
    public async Task<List<NearbyWashroomDto>> GetNearByAsync(double latitude, double longitude)
    {
        var activeWashrooms = await dbContext.Washrooms.Where(washroom => washroom.IsActive).ToListAsync();

        var washroomsWithDistance = activeWashrooms.Select(washroom =>
        {
            var distance = CalculateDistanceKm(
             latitude, longitude,
             washroom.Latitude, washroom.Longitude
            );



            return new NearbyWashroomDto(
                washroom.Id,
                washroom.Name,
                washroom.Address,
                washroom.Latitude,
                washroom.Longitude,
                distance,
                washroom.Type,
                washroom.Status,
                washroom.Hours,
                washroom.IsAccessible
            );
        });

        var nearbyWashrooms = washroomsWithDistance
        .Where(dto => dto.DistanceKm <= 3)
        .OrderBy(dto => dto.DistanceKm).ToList();

        return nearbyWashrooms;

    }
    private static double CalculateDistanceKm(double userLatitude, double userLongitude, double washroomLatitude, double washroomLongitude)
    {
        const double EarthRadiusKm = 6371;

        var userLatRad = userLatitude * Math.PI / 180;
        var washroomLatRad = washroomLatitude * Math.PI / 180;


        var deltaLat = (washroomLatitude - userLatitude) * Math.PI / 180;
        var deltaLon = (washroomLongitude - userLongitude) * Math.PI / 180;
        var a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(userLatRad) * Math.Cos(washroomLatRad) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a)
        );

        return EarthRadiusKm * c;

    }

}