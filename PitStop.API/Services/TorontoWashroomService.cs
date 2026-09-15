using PitStop.API.Data;
using System.Text.Json;
using PitStop.API.Dtos;

namespace PitStop.API.Services;

public class TorontoWashroomService(HttpClient httpClient, PitStopDbContext dbContext)
{

    public async Task GetTorontoWashroomsAsync()
    {

        var response = await httpClient.GetAsync("https://ckan0.cf.opendata.inter.prod-toronto.ca/api/3/action/datastore_search?id=1c7d1063-2562-4de3-8cd3-4cef48419f6f");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var torontoResponse =
            JsonSerializer.Deserialize<TorontoWashroomResponse>(json);

        if (torontoResponse is null)
        {
            return;
        }
        var torontoWashrooms = torontoResponse.Result.Records;
    }



}