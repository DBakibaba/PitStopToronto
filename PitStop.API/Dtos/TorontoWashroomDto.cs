using System.Text.Json.Serialization;

namespace PitStop.API.Dtos;

public record TorontoWashroomDto
(
    [property:JsonPropertyName("ASSET_ID")]
    int AssetId,
    [property:JsonPropertyName("AssetName")]
    string AssetName,
    [property:JsonPropertyName("ADDRESS")]
    string? Address,
    [property: JsonPropertyName("Status")]
    string Status,
    [property:JsonPropertyName("geometry")]
    string Geometry,
    [property:JsonPropertyName("TYPE")]
    string Type,
    [property:JsonPropertyName("HOURS")]
    string? Hours,
    [property:JsonPropertyName("ACCESSIBLE_FEATURES")]
    string Accessible_Features

);

public record TorontoWashroomResult(
    [property: JsonPropertyName("records")]
        List<TorontoWashroomDto> Records

);

public record TorontoWashroomResponse(
    [property: JsonPropertyName("success")]
    bool Success,

    [property: JsonPropertyName("result")]
    TorontoWashroomResult Result
);