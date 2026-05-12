using Catch_your_bus.Models.Entities;

namespace Catch_your_bus.Models.Services;

public interface IGoogleRoutesService
{
    /// <summary>
    /// Calls the Google Routes API (computeRoutes, travelMode=TRANSIT)
    /// and returns a parsed <see cref="CalculatedRoute"/>, or null if no route was found.
    /// </summary>
    Task<CalculatedRoute?> GetTransitRouteAsync(double startLat, double startLng, double endLat, double endLng);
}
