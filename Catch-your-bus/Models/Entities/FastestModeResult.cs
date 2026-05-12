namespace Catch_your_bus.Models.Entities;

/// <summary>
/// Result of comparing transit vs. pure foot travel to determine the overall fastest approach.
/// </summary>
public class FastestModeResult
{
    public required CalculatedRoute TransitRoute { get; set; }
    public required CalculatedRoute FootRoute { get; set; }
    public required CalculatedRoute Winner { get; set; }
    public TimeSpan Difference { get; set; }
}
