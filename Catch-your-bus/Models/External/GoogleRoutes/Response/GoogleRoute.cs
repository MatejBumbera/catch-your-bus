using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

public class GoogleRoute
{
    /// <summary>
    /// A duration in seconds with up to nine fractional digits, ending with 's'. (e.g. "1234s").
    /// </summary>
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("distanceMeters")]
    public int? DistanceMeters { get; set; }

    [JsonPropertyName("legs")]
    public List<GoogleLeg>? Legs { get; set; }
}
