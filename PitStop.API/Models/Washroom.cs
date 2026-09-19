namespace PitStop.API.Models;

public class Washroom
{

    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public LocationType Type { get; set; }
    public FacilityStatus Status { get; set; }
    public string? Hours { get; set; }
    public required string Source { get; set; }
    public bool IsActive { get; set; }
    public bool IsAccessible { get; set; }
    public string? ExternalId { get; set; }

}