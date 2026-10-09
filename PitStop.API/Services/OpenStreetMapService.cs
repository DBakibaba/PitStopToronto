using Microsoft.EntityFrameworkCore;
namespace PitStop.API.Services
{
    public class OpenStreetMapService(HttpClient httpClient)
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

            Console.WriteLine($"Overpass status: {(int)response.StatusCode}");
            Console.WriteLine(json);

            response.EnsureSuccessStatusCode();

            return json;

        }
    }
}
