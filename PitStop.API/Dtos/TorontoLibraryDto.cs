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
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    double? Latitude,

    [property: JsonPropertyName("Long")]
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    double? Longitude,

    [property: JsonPropertyName("Hours")]
    string? Hours,

    [property: JsonPropertyName("PublicParking")]
    string PublicParking
);

public record TorontoLibraryResult(
    [property:JsonPropertyName
    ("records")]
        List<TorontoLibraryDto> Records
);

public record TorontoLibraryResponse(
    [property:JsonPropertyName
    ("success")]
    bool Success,

    [property: JsonPropertyName("result")]
    TorontoLibraryResult Result
);