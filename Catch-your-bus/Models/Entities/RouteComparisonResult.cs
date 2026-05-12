namespace Catch_your_bus.Models.Entities;

/// <summary>
/// Result of comparing a standard transit route against a running alternative.
/// </summary>
public class RouteComparisonResult
{
    public CalculatedRoute StandardRoute { get; set; } = null!;

    /// <summary>
    /// Null if no meaningful running alternative was found
    /// (e.g. no walking segment, or running doesn't help to catch earlier bus).
    /// </summary>
    public CalculatedRoute? RunningAlternative { get; set; }

    /// <summary>
    /// How much time the running alternative saves compared to the standard route.
    /// </summary>
    public TimeSpan TimeSaved { get; set; }
}
