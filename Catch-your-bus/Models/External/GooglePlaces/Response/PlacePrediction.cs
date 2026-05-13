using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GooglePlaces;

public class PlacePrediction
{
    [JsonPropertyName("placeId")]
    public string PlaceId { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public PlacePredictionText? Text { get; set; }
    
    public string DisplayText => Text?.Text ?? PlaceId;
}
