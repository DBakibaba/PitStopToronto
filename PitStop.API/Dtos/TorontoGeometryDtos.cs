using System.Text.Json.Serialization;

namespace PitStop.API.Dtos;

public record TorontoGeometryDto(
    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("coordinates")]
    double[] Coordinates
);