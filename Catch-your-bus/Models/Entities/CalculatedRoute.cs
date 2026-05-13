namespace Catch_your_bus.Models.Entities;

/// <summary>
/// DTO representing a fully calculated route from origin to destination,
/// composed of one or more <see cref="RouteSegment"/>s.
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
    
    public List<RouteSegment> Segments { get; set; } = new();

    /// <summary>
    /// The type of route calculation that produced this result.
    /// </summary>
    public RouteType Type { get; set; }
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
