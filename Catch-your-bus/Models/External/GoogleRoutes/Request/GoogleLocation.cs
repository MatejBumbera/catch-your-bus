using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Request;

public class GoogleLocation
{
    [JsonPropertyName("latLng")]
    public GoogleLatLng LatLng { get; set; } = new();
}