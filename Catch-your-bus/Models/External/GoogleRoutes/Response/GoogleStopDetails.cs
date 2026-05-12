using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes;

public class GoogleStopDetails
{
    [JsonPropertyName("departureStop")]
    public GoogleTransitStop? DepartureStop { get; set; }

    [JsonPropertyName("arrivalStop")]
    public GoogleTransitStop? ArrivalStop { get; set; }
    
    [JsonPropertyName("departureTime")]
    public DateTimeOffset? DepartureTime { get; set; }
    
    [JsonPropertyName("arrivalTime")]
    public DateTimeOffset? ArrivalTime { get; set; }
}
