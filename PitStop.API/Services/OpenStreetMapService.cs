using Microsoft.EntityFrameworkCore;
using PitStop.API.Data;
using PitStop.API.Dtos;
using System.Text.Json;
using PitStop.API.Models;

namespace PitStop.API.Services
{
    public class OpenStreetMapService(HttpClient httpClient, PitStopDbContext dbContext)
    {
        public async Task<(int Imported, int Updated)> GetLoblawsLocationsAsync()
        {
            string query = """
        [out:json][timeout:25];
        nwr["shop"="supermarket"]["name"="Loblaws"](around:200000,43.7001,-79.4163);
        out center;
        """;

            string url = "https://overpass-api.de/api/interpreter?data="
                + Uri.EscapeDataString(query);

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var osmResponse =
                JsonSerializer.Deserialize<OpenStreetMapResponseDto>(json);

            if (osmResponse is null)
            {
                throw new InvalidOperationException("OSM returned no response object.");
            }

            var existingLocations = await dbContext.Washrooms
                .Where(washroom => washroom.Source == "OpenStreetMap")
                .ToListAsync();

            int importedCount = 0;
            int updatedCount = 0;

            foreach (var supermarket in osmResponse.Elements)
            {
                double? latitude = supermarket.Lat;
                double? longitude = supermarket.Lon;

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    latitude = supermarket.Center?.Lat;
                    longitude = supermarket.Center?.Lon;
                }

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    continue;
                }

                if (supermarket.Tags is null ||
                    string.IsNullOrWhiteSpace(supermarket.Tags.Name) ||
                    string.IsNullOrWhiteSpace(supermarket.Type))
                {
                    continue;
                }

                string externalId = $"{supermarket.Type}/{supermarket.Id}";

                var existingLocation = existingLocations.FirstOrDefault(
                    washroom => washroom.ExternalId == externalId);

                if (existingLocation is not null)
                {
                    existingLocation.Name = supermarket.Tags.Name;
                    existingLocation.Latitude = latitude.Value;
                    existingLocation.Longitude = longitude.Value;
                    existingLocation.Type = LocationType.GroceryStore;
                    existingLocation.Hours = supermarket.Tags.OpeningHours;
                    existingLocation.IsActive = true;

                    updatedCount++;
                    continue;
                }

                var washroom = new Washroom
                {
                    Name = supermarket.Tags.Name,
                    Source = "OpenStreetMap",
                    ExternalId = externalId,
                    Latitude = latitude.Value,
                    Longitude = longitude.Value,
                    Type = LocationType.GroceryStore,
                    Status = FacilityStatus.Unknown,
                    Hours = supermarket.Tags.OpeningHours,
                    IsActive = true
                };

                dbContext.Washrooms.Add(washroom);
                existingLocations.Add(washroom);
                importedCount++;
            }

            await dbContext.SaveChangesAsync();

            return (importedCount, updatedCount);
        }
    }

}
    
