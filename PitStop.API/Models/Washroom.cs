namespace PitStop.API.Models;


public class Washroom
{

    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public LocationType Type { get; set; }
    public FacilityStatus Status { get; set; }
    public int Hours { get; set; }
    public string Source { get; set; }
    public bool IsActive { get; set; }
    public bool IsAccessible { get; set; }

}