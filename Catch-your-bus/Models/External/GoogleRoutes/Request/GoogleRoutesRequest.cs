using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Request;

public class GoogleRoutesRequest
{
    [JsonPropertyName("origin")]
    public GoogleWaypoint Origin { get; set; } = new();

    [JsonPropertyName("destination")]
    public GoogleWaypoint Destination { get; set; } = new();

    [JsonPropertyName("travelMode")]
    public string TravelMode { get; set; } = string.Empty;
    
    [JsonPropertyName("departureTime")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DepartureTime { get; set; }
}