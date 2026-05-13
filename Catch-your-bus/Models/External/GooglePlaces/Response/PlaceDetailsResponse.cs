using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GooglePlaces.Response;

public class PlaceDetailsResponse
{
    [JsonPropertyName("location")]
    public PlaceLocation? Location { get; set; }
}
