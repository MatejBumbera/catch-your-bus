using Catch_your_bus.Models.Entities;

namespace Catch_your_bus.Models.Services;

public class RouteCalculationService(
    IGoogleRoutesService googleRoutes,
    UserPreferencesService preferences) : IRouteCalculationService
{
    private const double MinWalkingDistanceMeters = 50.0;

    public async Task<RouteOptionsBundle> CalculateAllRoutesAsync(
        double startLat, double startLng,
        double endLat, double endLng)
    {
        var bundle = new RouteOptionsBundle();
        var now = DateTimeOffset.Now;

        var walkTask = googleRoutes.GetWalkingRouteAsync(startLat, startLng, endLat, endLng);
        var transitTask = googleRoutes.GetTransitRouteAsync(startLat, startLng, endLat, endLng);

        await Task.WhenAll(walkTask, transitTask);

        bundle.StandardTransit = transitTask.Result;

        if (walkTask.Result is not null)
        {
            bundle.PureWalking = walkTask.Result;
            AdjustFootRouteSpeeds(bundle.PureWalking, preferences.WalkingSpeedKmH, now, RouteType.PureWalking);

            bundle.PureRunning = CreateRunningRoute(bundle.PureWalking, preferences.RunningSpeedKmH, now);
        }

        var firstWalk = bundle.StandardTransit?.Segments.FirstOrDefault(s => s.Mode == TravelMode.Walking);

        if (firstWalk is not null && firstWalk.DistanceMeters >= MinWalkingDistanceMeters)
        {
            var runningDuration = CalculateDuration(firstWalk.DistanceMeters, preferences.RunningSpeedKmH);
            var timeSaved = firstWalk.Duration - runningDuration;

            if (timeSaved.TotalSeconds >= 5)
            {
                var timeMachineDeparture = now.Subtract(timeSaved);

                var alternateRoute = await googleRoutes.GetTransitRouteAsync(
                    startLat, startLng, endLat, endLng, timeMachineDeparture);

                if (alternateRoute is not null)
                {
                    if (alternateRoute.ArrivalTime < bundle.StandardTransit?.ArrivalTime)
                    {
                        ChangeFirstWalkToRunAndFixTimeline(alternateRoute, runningDuration, now);
                        bundle.RunningAlternative = alternateRoute;
                    }
                }
            }
        }

        bundle.Winner = DetermineWinner(bundle);

        return bundle;
    }

    private static TimeSpan CalculateDuration(double distanceMeters, double speedKmH)
    {
        var hours = (distanceMeters / 1000.0) / speedKmH;
        return TimeSpan.FromHours(hours);
    }

    private static void AdjustFootRouteSpeeds(CalculatedRoute route, double speedKmH, DateTimeOffset startTime,
        RouteType type)
    {
        var currentTimeline = startTime;

        foreach (var segment in route.Segments)
        {
            segment.Duration = CalculateDuration(segment.DistanceMeters, speedKmH);
            segment.StartTime = currentTimeline;

            currentTimeline += segment.Duration;
            segment.EndTime = currentTimeline;
        }

        route.DepartureTime = startTime;
        route.ArrivalTime = currentTimeline;
        route.TotalDuration = currentTimeline - startTime;
        route.Type = type;
    }

    private static CalculatedRoute CreateRunningRoute(CalculatedRoute walkingRoute, double runSpeedKmH,
        DateTimeOffset now)
    {
        var runningSegments = walkingRoute.Segments.Select(s => new RouteSegment
        {
            Mode = TravelMode.Running,
            Instruction = "Run",
            DistanceMeters = s.DistanceMeters
        }).ToList();

        var runRoute = new CalculatedRoute
        {
            TotalDistanceMeters = walkingRoute.TotalDistanceMeters,
            Summary = "Run directly to destination",
            Segments = runningSegments
        };

        AdjustFootRouteSpeeds(runRoute, runSpeedKmH, now, RouteType.PureRunning);
        return runRoute;
    }

    private static void ChangeFirstWalkToRunAndFixTimeline(CalculatedRoute route, TimeSpan runningDuration,
        DateTimeOffset now)
    {
        var firstWalk = route.Segments.FirstOrDefault();
        if (firstWalk is null || firstWalk.Mode != TravelMode.Walking) return;
        
        firstWalk.Mode = TravelMode.Running;
        firstWalk.Instruction = "Run";
        firstWalk.Duration = runningDuration;
        firstWalk.StartTime = now;
        firstWalk.EndTime = now.Add(runningDuration);
        
        if (route.Segments.Count > 1)
        {
            var nextSegment = route.Segments[1];
            if (nextSegment.StartTime.HasValue)
            {
                var newWaitTime = nextSegment.StartTime.Value - firstWalk.EndTime.Value;
                nextSegment.WaitTimeAtDeparture = newWaitTime > TimeSpan.Zero ? newWaitTime : TimeSpan.Zero;
            }
        }
        
        route.DepartureTime = now;
        if (route.ArrivalTime.HasValue)
        {
            route.TotalDuration = route.ArrivalTime.Value - now;
        }

        route.Type = RouteType.RunningAlternative;
    }

    private static CalculatedRoute? DetermineWinner(RouteOptionsBundle bundle)
    {
        var validRoutes = new List<CalculatedRoute>();
        if (bundle.StandardTransit is not null) validRoutes.Add(bundle.StandardTransit);
        if (bundle.RunningAlternative is not null) validRoutes.Add(bundle.RunningAlternative);
        if (bundle.PureWalking is not null) validRoutes.Add(bundle.PureWalking);
        if (bundle.PureRunning is not null) validRoutes.Add(bundle.PureRunning);

        return validRoutes
            .Where(r => r.ArrivalTime.HasValue)
            .OrderBy(r => r.ArrivalTime!.Value)
            .FirstOrDefault();
    }
}