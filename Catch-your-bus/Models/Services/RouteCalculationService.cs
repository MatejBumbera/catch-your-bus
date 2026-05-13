using Catch_your_bus.Models.Entities;

namespace Catch_your_bus.Models.Services;

public class RouteCalculationService(
    IGoogleRoutesService googleRoutes,
    UserPreferencesService preferences) : IRouteCalculationService
{
    private const double MinWalkingDistanceMeters = 50.0;
    
    public async Task<RouteComparisonResult?> CalculateRoutesAsync(double startLat, double startLng,
        double endLat, double endLng)
    {
        var standardRoute = await googleRoutes.GetTransitRouteAsync(startLat, startLng, endLat, endLng);

        if (standardRoute is null)
        {
            return null;
        }

        var result = new RouteComparisonResult { StandardRoute = standardRoute };

        var firstWalk = standardRoute.Segments.FirstOrDefault(s => s.Mode == TravelMode.Walking);
        if (firstWalk is null || firstWalk.DistanceMeters < MinWalkingDistanceMeters)
        {
            return result; 
        }

        var runningDuration = CalculateDuration(firstWalk.DistanceMeters, preferences.RunningSpeedKmH);
        var timeSaved = firstWalk.Duration - runningDuration;

        if (timeSaved <= TimeSpan.Zero)
        {
            return result;
        }

        var timeMachineRoute = await googleRoutes.GetTransitRouteAsync(
            startLat, startLng, endLat, endLng, DateTimeOffset.UtcNow - timeSaved);

        if (timeMachineRoute is null || !IsBetterRoute(standardRoute, timeMachineRoute))
        {
            return result;
        }

        ChangeFirstWalkToRun(timeMachineRoute, runningDuration);
        
        result.RunningAlternative = timeMachineRoute;
        result.TimeSaved = standardRoute.TotalDuration - timeMachineRoute.TotalDuration;

        return result;
    }

    public async Task<CalculatedRoute?> GetPureFootModeRouteAsync(
        double startLat, double startLng,
        double endLat, double endLng,
        bool running = false)
    {
        var walkRoute = await googleRoutes.GetWalkingRouteAsync(startLat, startLng, endLat, endLng);

        if (walkRoute is null) 
        {
            return null;
        }

        var speedKmH = running ? preferences.RunningSpeedKmH : preferences.WalkingSpeedKmH;
        
        foreach (var segment in walkRoute.Segments.Where(s => s.Mode == TravelMode.Walking))
        {
            segment.Duration = CalculateDuration(segment.DistanceMeters, speedKmH);
            if (running) 
            {
                segment.Mode = TravelMode.Running;
            }
        }

        walkRoute.TotalDuration = CalculateDuration(walkRoute.TotalDistanceMeters, speedKmH);
        walkRoute.Type = running ? RouteType.PureRunning : RouteType.PureWalking;

        return walkRoute;
    }

    public async Task<FastestModeResult?> CompareFastestModeAsync(
        double startLat, double startLng,
        double endLat, double endLng)
    {
        var transitTask = CalculateRoutesAsync(startLat, startLng, endLat, endLng);
        var footTask = GetPureFootModeRouteAsync(startLat, startLng, endLat, endLng, running: true);

        var transitResult = await transitTask;
        var footRoute = await footTask;
        
        if (transitResult is null || footRoute is null) 
        {
            return null;
        }
        
        var bestTransitRoute = transitResult.RunningAlternative ?? transitResult.StandardRoute;
        
        var winner = bestTransitRoute.TotalDuration <= footRoute.TotalDuration 
            ? bestTransitRoute 
            : footRoute;
        
        return new FastestModeResult
        {
            TransitRoute = bestTransitRoute,
            FootRoute = footRoute,
            Winner = winner,
            Difference = (bestTransitRoute.TotalDuration - footRoute.TotalDuration).Duration()
        };
    }

    private static TimeSpan CalculateDuration(double distanceMeters, double speedKmH)
    {
        var hours = (distanceMeters / 1000.0) / speedKmH;
        return TimeSpan.FromHours(hours);
    }

    private static bool IsBetterRoute(CalculatedRoute standard, CalculatedRoute timeMachine)
    {
        var standardDeparture = standard.Segments.FirstOrDefault(s => s.Mode == TravelMode.Transit)?.StartTime;
        var tmDeparture = timeMachine.Segments.FirstOrDefault(s => s.Mode == TravelMode.Transit)?.StartTime;

        return tmDeparture < standardDeparture && timeMachine.TotalDuration < standard.TotalDuration;
    }

    private static void ChangeFirstWalkToRun(CalculatedRoute route, TimeSpan runningDuration)
    {
        var firstWalk = route.Segments.FirstOrDefault(s => s.Mode == TravelMode.Walking);
        if (firstWalk is null) 
        {
            return;
        }

        route.TotalDuration = route.TotalDuration - firstWalk.Duration + runningDuration;
        firstWalk.Duration = runningDuration;
        firstWalk.Mode = TravelMode.Running;
        route.Type = RouteType.RunningAlternative;
    }
}