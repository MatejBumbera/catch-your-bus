namespace Catch_your_bus.Models.Entities;

public class RouteOptionsBundle
{
    public CalculatedRoute? StandardTransit { get; set; }
    public CalculatedRoute? RunningAlternative { get; set; }
    
    public CalculatedRoute? PureWalking { get; set; }
    public CalculatedRoute? PureRunning { get; set; }

    /// <summary>
    /// The absolute fastest route out of all calculated options.
    /// </summary>
    public CalculatedRoute? Winner { get; set; }

    /// <summary>
    /// True if running to the bus stop actually gets you to your final destination earlier than standard transit.
    /// </summary>
    public bool IsRunningAlternativeBetter =>
        RunningAlternative?.ArrivalTime is not null &&
        (StandardTransit?.ArrivalTime is null || RunningAlternative.ArrivalTime < StandardTransit.ArrivalTime);

    /// <summary>
    /// The exact amount of time saved if the user chooses to run to the bus instead of walking.
    /// </summary>
    public TimeSpan RunningAlternativeTimeSaved => IsRunningAlternativeBetter && StandardTransit?.ArrivalTime is not null
        ? StandardTransit.ArrivalTime.Value - RunningAlternative!.ArrivalTime!.Value
        : TimeSpan.Zero;
}