using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes;

public class GoogleLatLng
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}