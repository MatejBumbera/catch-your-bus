using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GoogleTransitVehicleType
{
    TRANSIT_VEHICLE_TYPE_UNSPECIFIED,
    BUS,
    CABLE_CAR,
    COMMUTER_TRAIN,
    FERRY,
    FUNICULAR,
    GONDOLA_LIFT,
    HEAVY_RAIL,
    HIGH_SPEED_TRAIN,
    INTERCITY_BUS,
    LONG_DISTANCE_TRAIN,
    METRO_RAIL,
    MONORAIL,
    OTHER,
    RAIL,
    SHARE_TAXI,
    SUBWAY,
    TRAM,
    TROLLEYBUS
}