namespace Catch_your_bus.Models.Entities;

/// <summary>
/// DTO representing a fully calculated route from origin to destination.
/// </summary>
public class CalculatedRoute
{
    /// <summary>
    /// Human-readable summary of the route (e.g. "Transit via Tram 1 → Bus 67").
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    
    public string StartLocationName { get; set; } = string.Empty;
    public string EndLocationName { get; set; } = string.Empty;
    
    public TimeSpan TotalDuration { get; set; }
    public double TotalDistanceMeters { get; set; }

    public DateTimeOffset? DepartureTime { get; set; }
    public DateTimeOffset? ArrivalTime { get; set; }

    public List<RouteSegment> Segments { get; set; } = [];

    public RouteType Type { get; set; }
    
    /// <summary>
    /// How many distinct transit vehicles the user must board.
    /// </summary>
    public int TransitLegCount => Segments.Count(s => s.Mode == TravelMode.Transit);
    
    /// <summary>
    /// True if the user has to switch vehicles at least once.
    /// </summary>
    public bool RequiresTransfers => TransitLegCount > 1;
}

/// <summary>
/// Identifies the type/mode of route calculation.
/// </summary>
public enum RouteType
{
    /// <summary>
    /// Standard transit route as returned by Google Routes API.
    /// </summary>
    StandardTransit,

    /// <summary>
    /// Transit route with walking segments replaced by running.
    /// </summary>
    RunningAlternative,

    /// <summary>
    /// Pure walking route (no transit).
    /// </summary>
    PureWalking,

    /// <summary>
    /// Pure running route (no transit).
    /// </summary>
    PureRunning,
}
