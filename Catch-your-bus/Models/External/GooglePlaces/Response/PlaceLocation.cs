using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GooglePlaces.Response;

public class PlaceLocation
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}
