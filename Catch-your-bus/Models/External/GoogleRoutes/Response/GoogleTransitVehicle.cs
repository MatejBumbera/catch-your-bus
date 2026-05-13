using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

public class GoogleTransitVehicle
{
    [JsonPropertyName("type")]
    public GoogleTransitVehicleType? Type { get; set; }
}
