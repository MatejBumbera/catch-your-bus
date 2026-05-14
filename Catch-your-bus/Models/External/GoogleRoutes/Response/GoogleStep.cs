using System.Text.Json.Serialization;
using Catch_your_bus.Models.External.GoogleRoutes.Request;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

public class GoogleStep
{
    [JsonPropertyName("travelMode")]
    public GoogleRouteTravelMode? TravelMode { get; set; }

    /// <summary>
    /// A duration in seconds with up to nine fractional digits, ending with 's'.
    /// </summary>
    [JsonPropertyName("staticDuration")]
    public string? StaticDuration { get; set; }

    [JsonPropertyName("distanceMeters")]
    public int? DistanceMeters { get; set; }

    [JsonPropertyName("navigationInstruction")]
    public GoogleNavigationInstruction? NavigationInstruction { get; set; }

    [JsonPropertyName("transitDetails")]
    public GoogleTransitDetails? TransitDetails { get; set; }
    
    [JsonPropertyName("startLocation")]
    public GoogleLocation? StartLocation { get; set; }
    
    [JsonPropertyName("endLocation")]
    public GoogleLocation? EndLocation { get; set; }
}
