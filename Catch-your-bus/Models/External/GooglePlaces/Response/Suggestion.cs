using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GooglePlaces.Response;

public class Suggestion
{
    [JsonPropertyName("placePrediction")]
    public PlacePrediction? PlacePrediction { get; set; }
}
