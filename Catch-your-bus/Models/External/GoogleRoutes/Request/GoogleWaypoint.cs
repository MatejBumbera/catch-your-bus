using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Request;

public class GoogleWaypoint
{
    [JsonPropertyName("location")]
    public GoogleLocation Location { get; set; } = new();
}