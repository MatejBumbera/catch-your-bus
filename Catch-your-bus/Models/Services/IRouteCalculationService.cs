using Catch_your_bus.Models.Entities;

namespace Catch_your_bus.Models.Services;

public interface IRouteCalculationService
{
    /// <summary>
    /// The core "Time Machine" algorithm: fetches a standard transit route, then attempts to find
    /// an earlier bus by simulating a past departure time equal to the time saved by running.
    /// </summary>
    Task<RouteComparisonResult?> CalculateRoutesAsync(
        double startLat, double startLng,
        double endLat, double endLng);

    Task<CalculatedRoute?> GetPureFootModeRouteAsync(
        double startLat, double startLng,
        double endLat, double endLng,
        bool running = false);
    
    Task<FastestModeResult?> CompareFastestModeAsync(
        double startLat, double startLng,
        double endLat, double endLng);
}
