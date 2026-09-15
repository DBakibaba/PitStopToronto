using System.Text.Json.Serialization;
using PitStop.API.Models;


public record TorontoWashroomDto
(
    [property:JsonPropertyName("ASSET_ID")]
    int Asset_Id,
    [property:JsonPropertyName("AssetName")]
    string Asset_Name,
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