using System.Globalization;
using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.External.GoogleRoutes.Response;

namespace Catch_your_bus.Models.Services;

/// <summary>
/// Maps Google Routes API DTOs into our internal domain models.
/// </summary>
public static class GoogleRouteMapper
{
    public static CalculatedRoute? MapToCalculatedRoute(GoogleRoutesResponse? response, DateTimeOffset routeStartTime)
    {
        var googleRoute = response?.Routes?.FirstOrDefault();
        if (googleRoute is null) return null;

        var allSteps = googleRoute.Legs?
            .SelectMany(leg => leg.Steps ?? Enumerable.Empty<GoogleStep>())
            .ToList() ?? [];

        var segments = AggregateSteps(routeStartTime, allSteps);

        var route = new CalculatedRoute
        {
            TotalDuration = ParseProtobufDuration(googleRoute.Duration),
            TotalDistanceMeters = googleRoute.DistanceMeters ?? 0,
            Segments = segments,
            Summary = BuildSummary(segments),
            DepartureTime = segments.FirstOrDefault()?.StartTime,
            ArrivalTime = segments.LastOrDefault()?.EndTime
        };

        return route;
    }

    private static List<RouteSegment> AggregateSteps(DateTimeOffset routeStartTime, List<GoogleStep> allSteps)
    {
        var segments = new List<RouteSegment>();
        RouteSegment? pendingWalk = null;
        var currentTimeline = routeStartTime;
        
        foreach (var step in allSteps)
        {
            var isWalk = step.TravelMode == GoogleRouteTravelMode.WALK;
            var stepDuration = ParseProtobufDuration(step.StaticDuration);
            var stepDistance = step.DistanceMeters ?? 0;

            if (isWalk)
            {
                pendingWalk ??= new RouteSegment
                {
                    Mode = TravelMode.Walking,
                    Instruction = "Walk",
                    StartTime = currentTimeline,
                    Duration = TimeSpan.Zero,
                    DistanceMeters = 0
                };

                pendingWalk.Duration += stepDuration;
                pendingWalk.DistanceMeters += stepDistance;
                currentTimeline += stepDuration;
                pendingWalk.EndTime = currentTimeline;
            }
            else
            {
                if (pendingWalk is not null)
                {
                    segments.Add(pendingWalk);
                    pendingWalk = null;
                }

                var transitSegment = MapTransitStep(step);
                
                if (transitSegment.StartTime.HasValue)
                {
                    var waitTime = transitSegment.StartTime.Value - currentTimeline;
                    transitSegment.WaitTimeAtDeparture = waitTime > TimeSpan.Zero ? waitTime : TimeSpan.Zero;
                    
                    currentTimeline = transitSegment.EndTime ?? (transitSegment.StartTime.Value + transitSegment.Duration);
                }
                else
                {
                    transitSegment.StartTime = currentTimeline;
                    currentTimeline += transitSegment.Duration;
                    transitSegment.EndTime = currentTimeline;
                }

                segments.Add(transitSegment);
            }
        }
        
        if (pendingWalk is not null)
        {
            segments.Add(pendingWalk);
        }

        return segments;
    }

    private static RouteSegment MapTransitStep(GoogleStep step)
    {
        var segment = new RouteSegment
        {
            Mode = TravelMode.Transit,
            Duration = ParseProtobufDuration(step.StaticDuration),
            DistanceMeters = step.DistanceMeters ?? 0,
            Instruction = step.NavigationInstruction?.Instructions ?? string.Empty
        };

        var details = step.TransitDetails;
        if (details is null) return segment;

        if (details.StopDetails is not null)
        {
            segment.DepartureStopName = details.StopDetails.DepartureStop?.Name;
            segment.ArrivalStopName = details.StopDetails.ArrivalStop?.Name;
            segment.StartTime = details.StopDetails.DepartureTime?.ToLocalTime();
            segment.EndTime = details.StopDetails.ArrivalTime?.ToLocalTime();
        }

        segment.NumberOfStops = details.StopCount;
        segment.Headsign = details.Headsign;

        if (details.TransitLine is null) return segment;
        segment.TransitLineName = details.TransitLine.NameShort ?? details.TransitLine.Name;
        segment.TransitVehicleType = MapVehicleType(details.TransitLine.Vehicle?.Type);
        segment.LineColor = details.TransitLine.Color;
        segment.LineTextColor = details.TransitLine.TextColor;

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
            {
                return "Walk";
            }

            if (s.TransitVehicleType.HasValue && !string.IsNullOrEmpty(s.TransitLineName))
            {
                return $"{s.TransitVehicleType} {s.TransitLineName}";
            }

            return s.TransitLineName ?? "Transit";
        });

        return string.Join(" → ", parts);
    }
}