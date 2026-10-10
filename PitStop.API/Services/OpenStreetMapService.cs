using Microsoft.EntityFrameworkCore;
using PitStop.API.Data;
using PitStop.API.Dtos;
using System.Text.Json;
using PitStop.API.Models;
using System.Xml.Serialization;

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



        public async Task<(int Imported, int Updated)> GetHomeDepotLocationAsync()
        {
            string query = """
            [out:json][timeout:25];
            nwr["shop"="doityourself"]["name"="The Home Depot"](around:200000,43.7001,-79.4163);
            out center;
            """;

            string url = "https://overpass-api.de/api/interpreter?data="
                + Uri.EscapeDataString(query);

            var response = await httpClient.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            //string json = await response.Content.ReadAsStringAsync();

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

            foreach (var retailStore in osmResponse.Elements)
            {
                double? latitude = retailStore.Lat;
                double? longitude = retailStore.Lon;

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    latitude = retailStore.Center?.Lat;
                    longitude = retailStore.Center?.Lon;
                }
                if (!latitude.HasValue || !longitude.HasValue)
                {
                    continue;
                }

                if (retailStore.Tags is null || string.IsNullOrWhiteSpace(retailStore.Tags.Name) || string.IsNullOrWhiteSpace(retailStore.Tags.Brand))
                {
                    continue;
                }

                string externalId = $"{retailStore.Type}/{retailStore.Id}";
                var existingLocation = existingLocations.FirstOrDefault(
                    washroom => washroom.ExternalId == externalId);

                if (existingLocation is not null)
                {
                    existingLocation.Name = retailStore.Tags.Name;
                    existingLocation.Latitude = latitude.Value;
                    existingLocation.Longitude = longitude.Value;
                    existingLocation.Type = LocationType.GroceryStore;
                    existingLocation.Hours = retailStore.Tags.OpeningHours;
                    existingLocation.IsActive = true;

                    updatedCount++;
                    continue;

                }

                var washroom = new Washroom
                {
                    Name = retailStore.Tags.Name,
                    Source = "OpenStreetMap",
                    ExternalId = externalId,
                    Latitude = latitude.Value,
                    Longitude = longitude.Value,
                    Type = LocationType.GroceryStore,
                    Status = FacilityStatus.Unknown,
                    Hours = retailStore.Tags.OpeningHours,
                    IsActive = true
                };

                dbContext.Washrooms.Add(washroom);
                existingLocations.Add(washroom);
                importedCount++;


            }
            await dbContext.SaveChangesAsync();

            return (importedCount, updatedCount);


        }


        public async Task<(int Imported, int Updated)> GetCanadianTireLocationsAsync()
        {
            string query = """
            [out:json][timeout:25];
            nwr["shop"="department_store"]["name"="Canadian Tire"](around:200000,43.7001,-79.4163);
            out center;
            """;
            string url = "https://overpass-api.de/api/interpreter?data="
                 + Uri.EscapeDataString(query);

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();


            var osmResponse = JsonSerializer.Deserialize<OpenStreetMapResponseDto>(json);

            if (osmResponse is null)
            {
                throw new InvalidOperationException("OSM returned no response object.");
            }

            var existingLocations = await dbContext.Washrooms
                .Where(washroom => washroom.Source == "OpenStreetMap")
                .ToListAsync();

            int importedCount = 0;
            int updatedCount = 0;

            foreach (var retailStore in osmResponse.Elements)
            {
                double? latitude = retailStore.Lat;
                double? longitude = retailStore.Lon;

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    latitude = retailStore.Center?.Lat;
                    longitude = retailStore.Center?.Lon;
                }

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    continue;
                }

                if (retailStore.Tags is null)
                {
                    continue;
                }

                string externalId = $"{retailStore.Type}/{retailStore.Id}";

                var existingLocation = existingLocations.FirstOrDefault(
                    washroom => washroom.ExternalId == externalId);


                if (existingLocation is not null)
                {
                    existingLocation.Name = retailStore.Tags.Name ?? "Canadian Tire";
                    existingLocation.Latitude = latitude.Value;
                    existingLocation.Longitude = longitude.Value;
                    existingLocation.Type = LocationType.RetailStore;
                    existingLocation.Hours = retailStore.Tags.OpeningHours;
                    existingLocation.IsActive = true;

                    updatedCount++;
                    continue;
                }

                var washroom = new Washroom
                {
                    Name = retailStore.Tags.Name ?? "Canadian Tire",
                    Source = "OpenStreetMap",
                    ExternalId = externalId,
                    Latitude = latitude.Value,
                    Longitude = longitude.Value,
                    Type = LocationType.RetailStore,
                    Status = FacilityStatus.Unknown,
                    Hours = retailStore.Tags.OpeningHours,
                    IsActive = true
                };

                dbContext.Washrooms.Add(washroom);
                existingLocations.Add(washroom);
                importedCount++;

            }
            await dbContext.SaveChangesAsync();

            return (importedCount, updatedCount);

        }


        public async Task<(int Imported, int Updated)> GetTimHortonsLocationsAsync()
        {
            string query = """
        [out:json][timeout:25];
        nwr["name"="Tim Hortons"]["drive_through"="yes"](around:200000,43.7001,-79.4163);
        out center;
        """;

            string url = "https://overpass-api.de/api/interpreter?data="
                + Uri.EscapeDataString(query);

            var response = await httpClient.GetAsync(url);

            string json = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

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

            foreach (var cafe in osmResponse.Elements)
            {
                double? latitude = cafe.Lat;
                double? longitude = cafe.Lon;

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    latitude = cafe.Center?.Lat;
                    longitude = cafe.Center?.Lon;
                }

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    continue;
                }

                if (cafe.Tags is null)
                {
                    continue;
                }

                string externalId = $"{cafe.Type}/{cafe.Id}";

                var existingLocation = existingLocations.FirstOrDefault(
                    washroom => washroom.ExternalId == externalId);

                if (existingLocation is not null)
                {
                    existingLocation.Name = cafe.Tags.Name ?? "Tim Hortons";
                    existingLocation.Latitude = latitude.Value;
                    existingLocation.Longitude = longitude.Value;
                    existingLocation.Type = LocationType.Restaurant;
                    existingLocation.Hours = cafe.Tags.OpeningHours;
                    existingLocation.IsActive = true;

                    updatedCount++;
                    continue;
                }

                var washroom = new Washroom
                {
                    Name = cafe.Tags.Name ?? "Tim Hortons",
                    Source = "OpenStreetMap",
                    ExternalId = externalId,
                    Latitude = latitude.Value,
                    Longitude = longitude.Value,
                    Type = LocationType.Restaurant,
                    Status = FacilityStatus.Unknown,
                    Hours = cafe.Tags.OpeningHours,
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
    
