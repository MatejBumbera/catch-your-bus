using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GoogleRouteTravelMode
{
    TRAVEL_MODE_UNSPECIFIED,
    DRIVE,
    BICYCLE,
    WALK,
    TWO_WHEELER,
    TRANSIT
}