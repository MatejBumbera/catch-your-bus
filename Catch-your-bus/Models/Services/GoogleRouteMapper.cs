using System.Globalization;
using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.External.GoogleRoutes;

namespace Catch_your_bus.Models.Services;

/// <summary>
/// Maps Google Routes API DTOs into our internal domain models.
/// </summary>
public static class GoogleRouteMapper
{
    public static CalculatedRoute? MapToCalculatedRoute(GoogleRoutesResponse? response)
    {
        var googleRoute = response?.Routes?.FirstOrDefault();
        if (googleRoute is null)
            return null;
        
        var segments = googleRoute.Legs?
            .SelectMany(leg => leg.Steps ?? Enumerable.Empty<GoogleStep>())
            .Select(MapStep)
            .ToList() ?? [];

        return new CalculatedRoute
        {
            Type = RouteType.StandardTransit,
            TotalDuration = ParseProtobufDuration(googleRoute.Duration),
            TotalDistanceMeters = googleRoute.DistanceMeters ?? 0,
            Segments = segments,
            Summary = BuildSummary(segments)
        };
    }

    private static RouteSegment MapStep(GoogleStep step)
    {
        var segment = new RouteSegment
        {
            Mode = step.TravelMode == GoogleRouteTravelMode.WALK ? TravelMode.Walking : TravelMode.Transit,
            Duration = ParseProtobufDuration(step.StaticDuration),
            DistanceMeters = step.DistanceMeters ?? 0,
            Instruction = step.NavigationInstruction?.Instructions ?? string.Empty
        };

        if (segment.Mode != TravelMode.Transit || step.TransitDetails is null) return segment;
        var details = step.TransitDetails;
        if (details.StopDetails is not null)
        {
            segment.DepartureStopName = details.StopDetails.DepartureStop?.Name;
            segment.ArrivalStopName = details.StopDetails.ArrivalStop?.Name;
            segment.DepartureTime = details.StopDetails.DepartureTime;
            segment.ArrivalTime = details.StopDetails.ArrivalTime;
        }

        segment.NumberOfStops = details.StopCount;

        if (details.TransitLine is null) return segment;
        segment.TransitLineName = details.TransitLine.NameShort ?? details.TransitLine.Name;
        segment.TransitVehicleType = MapVehicleType(details.TransitLine.Vehicle?.Type);

        return segment;
    }

    private static VehicleType MapVehicleType(GoogleTransitVehicleType? apiType) => apiType switch
    {
        GoogleTransitVehicleType.BUS or GoogleTransitVehicleType.INTERCITY_BUS => VehicleType.Bus,
        GoogleTransitVehicleType.TRAM or GoogleTransitVehicleType.TROLLEYBUS => VehicleType.Tram,
        GoogleTransitVehicleType.SUBWAY or GoogleTransitVehicleType.METRO_RAIL => VehicleType.Subway,
        GoogleTransitVehicleType.RAIL or GoogleTransitVehicleType.HEAVY_RAIL or GoogleTransitVehicleType.COMMUTER_TRAIN
            or GoogleTransitVehicleType.HIGH_SPEED_TRAIN or GoogleTransitVehicleType.LONG_DISTANCE_TRAIN
            or GoogleTransitVehicleType.MONORAIL => VehicleType.Rail,
        GoogleTransitVehicleType.FERRY => VehicleType.Ferry,
        _ => VehicleType.Other
    };
    
    /// <summary>
    /// Parses a protobuf Duration string (e.g. "1234s") into a TimeSpan.
    /// </summary>
    private static TimeSpan ParseProtobufDuration(string? raw) =>
        !string.IsNullOrEmpty(raw) && double.TryParse(raw.TrimEnd('s'), CultureInfo.InvariantCulture, out var seconds)
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.Zero;

    /// <summary>
    /// Builds a human-readable summary like "Walk → Tram 1 → Walk → Bus 67".
    /// </summary>
    private static string BuildSummary(List<RouteSegment> segments)
    {
        var parts = segments.Select(s =>
        {
            if (s.Mode == TravelMode.Walking)
                return "Walk";

            if (s.TransitVehicleType.HasValue && !string.IsNullOrEmpty(s.TransitLineName))
                return $"{s.TransitVehicleType} {s.TransitLineName}";

            return s.TransitLineName ?? "Transit";
        });

        return string.Join(" → ", parts);
    }
}
