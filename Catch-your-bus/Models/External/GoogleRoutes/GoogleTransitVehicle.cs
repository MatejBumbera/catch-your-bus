using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes;

public class GoogleTransitVehicle
{
    [JsonPropertyName("type")]
    public GoogleTransitVehicleType? Type { get; set; }
}
