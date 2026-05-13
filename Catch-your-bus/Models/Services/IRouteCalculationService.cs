using Catch_your_bus.Models.Entities;

namespace Catch_your_bus.Models.Services;

public interface IRouteCalculationService
{
    Task<RouteOptionsBundle> CalculateAllRoutesAsync(
        double startLat, double startLng, 
        double endLat, double endLng);
}
