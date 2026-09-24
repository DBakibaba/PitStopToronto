using Microsoft.EntityFrameworkCore;
using PitStop.API.Data;
using PitStop.API.Dtos;
using PitStop.API.Models;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Xml.Linq;



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
            if (existingLibrary is not null)
            {
                existingLibrary.Name = library.Name;
                existingLibrary.Address = library.Address;
                existingLibrary.Latitude = library.Latitude;
                existingLibrary.Longitude = library.Longitude;
                existingLibrary.Hours = library.Hours;
                existingLibrary.Type = LocationType.Library;
                existingLibrary.Source = "Toronto Library Data";
                existingLibrary.ExternalId = library.BranchCode;
                existingLibrary.IsActive = true;
                existingLibrary.Status = FacilityStatus.Unknown;
                existingLibrary.IsAccessible = true;
                continue;
            }
  
                var washroom = new Washroom
                {

                    Name = library.Name,
                    Address = library.Address,
                    Latitude = library.Latitude,
                    Longitude = library.Longitude,
                    Hours = library.Hours,
                    Type = LocationType.Library,
                    Source = "Toronto Library Data",
                    ExternalId = library.BranchCode,
                    IsActive = true,
                    Status = FacilityStatus.Unknown,
                    IsAccessible = true

                }; 
            

            dbContext.Washrooms.Add(washroom);
        }
        await dbContext.SaveChangesAsync();


    }

};