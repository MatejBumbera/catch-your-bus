using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes;

public class GoogleTransitDetails
{
    [JsonPropertyName("stopDetails")]
    public GoogleStopDetails? StopDetails { get; set; }

    [JsonPropertyName("transitLine")]
    public GoogleTransitLine? TransitLine { get; set; }

    [JsonPropertyName("stopCount")]
    public int? StopCount { get; set; }
}
