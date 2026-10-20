using PitStop.API.Models;

namespace PitStop.API.Dtos;

public record NearbyWashroomDto(
    int Id,
    string Name,
    string? Address,
    double Latitude,
    double Longitude,
    double DistanceKm,
    string Type,
    string HoursText
);
