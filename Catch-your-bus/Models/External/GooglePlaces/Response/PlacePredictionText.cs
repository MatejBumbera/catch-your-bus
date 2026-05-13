using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GooglePlaces;

public class PlacePredictionText
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
