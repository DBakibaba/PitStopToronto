
namespace PitStop.API.Services;

public static class DistanceCalculator
{
    public static double CalculateDistanceKm(
        double userLatitude,
        double userLongitude,
        double washroomLatitude,
        double washroomLongitude)
    {
        const double EarthRadiusKm = 6371;

        double userLatRad = userLatitude * Math.PI / 180;
        double washroomLatRad = washroomLatitude * Math.PI / 180;

        double deltaLat = (washroomLatitude - userLatitude) * Math.PI / 180;
        double deltaLon = (washroomLongitude - userLongitude) * Math.PI / 180;

        double a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(userLatRad) * Math.Cos(washroomLatRad) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a)
        );

        return EarthRadiusKm * c;
    }
}
