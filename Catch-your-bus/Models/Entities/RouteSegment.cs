namespace Catch_your_bus.Models.Entities;

public enum TravelMode
{
    Walking,
    Transit,
    Running
}

public enum VehicleType
{
    Bus,
    Tram,
    Subway,
    Rail,
    Ferry,
    Other
}

/// <summary>
/// DTO representing a single segment (step) within a calculated route.
/// </summary>
public class RouteSegment
{
    public TravelMode Mode { get; set; }

    /// <summary>
    /// Human-readable instruction (e.g. "Walk to Česká" or "Take Tram 1").
    /// </summary>
    public string Instruction { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }
    public double DistanceMeters { get; set; }
    
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    
    /// <summary>
    /// The time spent waiting at the stop *before* this segment begins.
    /// </summary>
    public TimeSpan? WaitTimeAtDeparture { get; set; }
    
    public string? TransitLineName { get; set; }
    public VehicleType? TransitVehicleType { get; set; }
    
    /// <summary>
    /// The direction the vehicle is traveling (e.g., "Sídliště Bystrc").
    /// </summary>
    public string? Headsign { get; set; } 
    
    /// <summary>
    /// Hex color code provided by the transit agency (e.g., "#FF0000").
    /// </summary>
    public string? LineColor { get; set; }
    
    /// <summary>
    /// Hex color code for the text on top of the LineColor.
    /// </summary>
    public string? LineTextColor { get; set; } 

    public string? DepartureStopName { get; set; }
    public string? ArrivalStopName { get; set; }
    public int? NumberOfStops { get; set; }
    public bool HasWaitTime => WaitTimeAtDeparture.HasValue;
}
