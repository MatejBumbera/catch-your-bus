using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes;

public class GoogleTransitLine
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Short line identifier (e.g. "1", "67", "M7"). Preferred over <see cref="Name"/> for display.
    /// </summary>
    [JsonPropertyName("nameShort")]
    public string? NameShort { get; set; }

    [JsonPropertyName("vehicle")]
    public GoogleTransitVehicle? Vehicle { get; set; }
}
