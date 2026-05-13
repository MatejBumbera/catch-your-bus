using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GooglePlaces;

public class Suggestion
{
    [JsonPropertyName("placePrediction")]
    public PlacePrediction? PlacePrediction { get; set; }
}
