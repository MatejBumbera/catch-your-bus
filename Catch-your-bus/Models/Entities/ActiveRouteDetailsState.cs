namespace Catch_your_bus.Models.Entities;

public class ActiveRouteDetailsState
{
    public CalculatedRoute? Route { get; set; }
    public DateTimeOffset? StandardRouteArrivalTime { get; set; }
}