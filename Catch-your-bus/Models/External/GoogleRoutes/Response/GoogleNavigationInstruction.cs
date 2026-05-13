using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

public class GoogleNavigationInstruction
{
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }
}
