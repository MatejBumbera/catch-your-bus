using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

public class GoogleLeg
{
    [JsonPropertyName("steps")]
    public List<GoogleStep>? Steps { get; set; }
}
