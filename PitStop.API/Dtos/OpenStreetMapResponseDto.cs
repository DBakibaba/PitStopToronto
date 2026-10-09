using System.Text.Json.Serialization;

namespace PitStop.API.Dtos
{
    public class OpenStreetMapResponseDto
    {
        [JsonPropertyName("elements")]
        public List<OpenStreetMapElementDto> Elements { get; set; } = new();
    }

    public class OpenStreetMapElementDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("lat")]
        public double? Lat { get; set; }

        [JsonPropertyName("lon")]
        public double? Lon { get; set; }

        [JsonPropertyName("tags")]
        public OpenStreetMapTagsDto? Tags { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class OpenStreetMapTagsDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("shop")]
        public string? Shop { get; set; }

        [JsonPropertyName("opening_hours")]
        public string? OpeningHours { get; set; }
    }
}