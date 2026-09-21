using PitStop.API.Data;
using System.Text.Json;
using PitStop.API.Dtos;
using PitStop.API.Models;
using Microsoft.EntityFrameworkCore;

namespace PitStop.API.Services;

public class TorontoWashroomService(HttpClient httpClient, PitStopDbContext dbContext)
{

    public async Task<(int Imported, int Updated)> GetTorontoWashroomsAsync()
    {

        var response = await httpClient.GetAsync("https://ckan0.cf.opendata.inter.prod-toronto.ca/api/3/action/datastore_search?id=1c7d1063-2562-4de3-8cd3-4cef48419f6f&limit=1000");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var torontoResponse =
            JsonSerializer.Deserialize<TorontoWashroomResponse>(json);

        if (torontoResponse is null)
        {
            return (0, 0);
        }
        var torontoWashrooms = torontoResponse.Result.Records;
        var importedCount = 0;
        var updatedCount = 0;
        var existingWashrooms = await dbContext.Washrooms
           .Where(existing => existing.Source == "Toronto Open Data").ToListAsync();


        foreach (var torontoWashroom in torontoWashrooms)
        {
            var geometry = JsonSerializer.Deserialize<TorontoGeometryDto>(torontoWashroom.Geometry);

            if (geometry is null)
            {
                continue;
            }
            var longitude = geometry.Coordinates[0];
            var latitude = geometry.Coordinates[1];

            var externalId = torontoWashroom.AssetId.ToString();
            var existingWashroom = existingWashrooms.FirstOrDefault(washroom => washroom.ExternalId == externalId);

            if (existingWashroom is not null)
            {
                existingWashroom.Status = MapStatus(torontoWashroom.Status);
                existingWashroom.Address = torontoWashroom.Address?.Trim();
                existingWashroom.Latitude = latitude;
                existingWashroom.Longitude = longitude;
                existingWashroom.Status = MapStatus(torontoWashroom.Status);
                existingWashroom.Hours = torontoWashroom.Hours?.Trim();
                existingWashroom.Type = MapLocationType(torontoWashroom.Type);
                existingWashroom.IsAccessible = MapAccessibility(torontoWashroom.AccessibleFeatures);
                existingWashroom.IsActive = true;
                updatedCount++;
                continue;
            }
            var washroom = new Washroom
            {

                Name = torontoWashroom.AssetName.Trim(),
                Address = torontoWashroom.Address?.Trim(),
                Latitude = latitude,
                Longitude = longitude,
                Status = MapStatus(torontoWashroom.Status),
                Source = "Toronto Open Data",
                Hours = torontoWashroom.Hours?.Trim(),
                Type = MapLocationType(torontoWashroom.Type),
                IsAccessible = MapAccessibility(torontoWashroom.AccessibleFeatures),
                ExternalId = torontoWashroom.AssetId.ToString(),
                IsActive = true,



            };

            dbContext.Washrooms.Add(washroom);
            importedCount++;
        }
        await dbContext.SaveChangesAsync();
        return (importedCount, updatedCount);
    }

    private FacilityStatus MapStatus(string status)
    {

        switch (status)
        {
            case "0":
                return FacilityStatus.Closed;

            case "1":
                return FacilityStatus.Open;

            case "2":
                return FacilityStatus.ServiceAlert;
            default:
                return FacilityStatus.ServiceAlert;


        }

    }

    private LocationType MapLocationType(string type)
    {
        switch (type)
        {
            case "Washroom Building":
                return LocationType.PublicWashroom;

            default:
                return LocationType.PublicWashroom;
        }
    }

    private bool MapAccessibility(string accessibleFeatures)
    {
        if (accessibleFeatures == "None")
        {
            return false;

        }
        return true;
    }
}