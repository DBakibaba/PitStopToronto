using Microsoft.EntityFrameworkCore;
using PitStop.API.Data;
using PitStop.API.Dtos;
using System.Text.Json;
using PitStop.API.Models;

namespace PitStop.API.Services
{
    public class OpenStreetMapService( HttpClient httpClient,PitStopDbContext dbContext)
    {
        public async Task<string> GetLoblawsLocationsAsync()
        {
            string query = """
            [out:json][timeout:25];
            nwr["shop"="supermarket"]["name"="Loblaws"](around:200000,43.7001,-79.4163);
            out center;
            """;

            string url = "https://overpass-api.de/api/interpreter?data=" + Uri.EscapeDataString(query);

            var response = await httpClient.GetAsync(url);

            string json = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var osmResponse =JsonSerializer.Deserialize<OpenStreetMapResponseDto>(json);

            if(osmResponse is null)
            {
                return json;

            }
            foreach (var supermarket in osmResponse.Elements)
            {
                if (!supermarket.Lat.HasValue || !supermarket.Lon.HasValue)
                {
                    continue;
                }
                if (supermarket.Tags is null || string.IsNullOrWhiteSpace(supermarket.Tags.Name))
                {
                    continue;
                }

                var washroom = new Washroom
                {
                    Name = supermarket.Tags.Name,
                    Source="OpenStreetMap",
                    Latitude = supermarket.Lat.Value,
                    Longitude = supermarket.Lon.Value,
                    Type = LocationType.GroceryStore,
                    Status = FacilityStatus.Unknown,
                    Hours = supermarket.Tags.OpeningHours,
                    IsActive = true
                }; 
            }


            return json;

        }
    }
}
