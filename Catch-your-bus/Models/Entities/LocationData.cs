namespace Catch_your_bus.Models.Entities;

/// <summary>
/// Simple DTO holding start/end coordinates for a route calculation request.
/// </summary>
public class LocationData
{
    public double StartLat { get; set; }
    public double StartLng { get; set; }
    public double EndLat { get; set; }
    public double EndLng { get; set; }
}
