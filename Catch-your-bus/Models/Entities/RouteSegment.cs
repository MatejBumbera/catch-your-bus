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
    /// Human-readable instruction for this segment (e.g. "Walk to Česká").
    /// </summary>
    public string Instruction { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }
    
    public double DistanceMeters { get; set; }

    /// <summary>
    /// Name of the transit line (e.g. "Tram 1", "Bus 67").
    /// Null if <see cref="Mode"/> is not <see cref="TravelMode.Transit"/>.
    /// </summary>
    public string? TransitLineName { get; set; }

    /// <summary>
    /// Null if <see cref="Mode"/> is not <see cref="TravelMode.Transit"/>.
    /// </summary>
    public VehicleType? TransitVehicleType { get; set; }

    /// <summary>
    /// Null if <see cref="Mode"/> is not <see cref="TravelMode.Transit"/>.
    /// </summary>
    public string? DepartureStopName { get; set; }

    /// <summary>
    /// Null if <see cref="Mode"/> is not <see cref="TravelMode.Transit"/>.
    /// </summary>
    public string? ArrivalStopName { get; set; }

    /// <summary>
    /// Null if <see cref="Mode"/> is not <see cref="TravelMode.Transit"/>.
    /// </summary>
    public int? NumberOfStops { get; set; }

    /// <summary>
    /// Scheduled departure time.
    /// Null if <see cref="Mode"/> is not <see cref="TravelMode.Transit"/>.
    /// </summary>
    public DateTimeOffset? DepartureTime { get; set; }

    /// <summary>
    /// Scheduled arrival time.
    /// Null if <see cref="Mode"/> is not <see cref="TravelMode.Transit"/>.
    /// </summary>
    public DateTimeOffset? ArrivalTime { get; set; }
}
