using PitStop.API.Data;
using System.Text.Json;
using PitStop.API.Dtos;
using PitStop.API.Models;
using Microsoft.EntityFrameworkCore;



namespace PitStop.API.Services;

public class TorontoLibraryService(HttpClient httpClient, PitStopDbContext dbContext)
{

    public async Task GetTorontoLibrariesAsync()
    {
        var response = await httpClient.GetAsync("https://ckan0.cf.opendata.inter.prod-toronto.ca/api/3/action/datastore_search?id=7420a950-e62b-41da-826c-32d31c46e8f8&limit=1000");
        response.EnsureSuccessStatusCode();


        var json = await response.Content.ReadAsStringAsync();
        var libraryResponse = JsonSerializer.Deserialize<TorontoLibraryResponse>(json);

        if (libraryResponse is null)
        {
            return;
        }

        var libraries = libraryResponse.Result.Records;
        var librariesWithParking = libraries.
            Where(l => l.PublicParking != "0").ToList();

        var existingLibraries = await dbContext.Washrooms
            .Where(w => w.Source == "Toronto Library Data").ToListAsync();

       foreach(var library in librariesWithParking)
        {
            var existingLibrary = existingLibraries.
                FirstOrDefault(l => l.ExternalId == library.BranchCode);
            var washroom = new Washroom
            {

                Name = library.Name,
                Address=library.Address,
                Latitude=library.Latitude,
                Longitude=library.Longitude,
                Hours=library.Hours,
                Type=LocationType.Library,
                Source="Toronto Library Data",
                ExternalId=library.BranchCode,
                IsActive=true,
                Status=FacilityStatus.Unknown,
                IsAccessible=true

            };

            dbContext.Washrooms.Add(washroom);
        }
        await dbContext.SaveChangesAsync();


    }

};