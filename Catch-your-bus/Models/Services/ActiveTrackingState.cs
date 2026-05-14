namespace Catch_your_bus.Models.Services;

public class ActiveTrackingState
{
    public string TargetBusStop { get; set; } = string.Empty;
    public DateTimeOffset DepartureTime { get; set; }
    public Location? DestinationLocation { get; set; }
}