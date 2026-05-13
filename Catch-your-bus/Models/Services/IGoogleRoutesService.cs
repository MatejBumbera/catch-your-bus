using Catch_your_bus.Models.Entities;

namespace Catch_your_bus.Models.Services;

/// <summary>
/// Api is documented at: https://developers.google.com/maps/documentation/routes/compute_route_directions
/// </summary>
public interface IGoogleRoutesService
{
    /// <summary>
    /// Calls the Google Routes API (computeRoutes, travelMode=TRANSIT).
    /// If <paramref name="departureTime"/> is null, the API defaults to "now".
    /// </summary>
    Task<CalculatedRoute?> GetTransitRouteAsync(
        double startLat, double startLng,
        double endLat, double endLng,
        DateTimeOffset? departureTime = null);

    /// <summary>
    /// Calls the Google Routes API (computeRoutes, travelMode=WALK).
    /// Returns a purely walking route without any transit.
    /// </summary>
    Task<CalculatedRoute?> GetWalkingRouteAsync(
        double startLat, double startLng,
        double endLat, double endLng);
}
