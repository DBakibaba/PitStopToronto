using System.Text.Json.Serialization;

namespace PitStop.API.Dtos;

public record TorontoLibraryDto
(
    [property: JsonPropertyName("BranchCode")]
    string BranchCode,

    [property: JsonPropertyName("BranchName")]
    string Name,

    [property: JsonPropertyName("Address")]
    string? Address,

    [property: JsonPropertyName("Lat")]
    double Latitude,

    [property: JsonPropertyName("Long")]
    double Longitude,

    [property: JsonPropertyName("Hours")]
    string? Hours,

    [property: JsonPropertyName("PublicParking")]
    string PublicParking
);