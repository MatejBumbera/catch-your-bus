using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

public class GoogleTransitLine
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nameShort")]
    public string? NameShort { get; set; }

    [JsonPropertyName("vehicle")]
    public GoogleTransitVehicle? Vehicle { get; set; }
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("textColor")]
    public string? TextColor { get; set; }
}