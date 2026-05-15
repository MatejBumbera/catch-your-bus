namespace Catch_your_bus.Models.Entities;

public class ActiveTrackingState
{
    public string TargetBusStop { get; set; } = string.Empty;
    public DateTimeOffset DepartureTime { get; set; }
    public Location? DestinationLocation { get; set; }
    public CalculatedRoute? FullRoute { get; set; }
    public DateTime? StandardRouteArrivalTime { get; set; }
}