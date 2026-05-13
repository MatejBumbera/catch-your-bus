using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

public class GoogleTransitStop
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
