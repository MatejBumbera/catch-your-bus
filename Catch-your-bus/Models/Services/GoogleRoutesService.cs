using System.Globalization;
using System.Net.Http.Json;
using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.External.GoogleRoutes;
using Catch_your_bus.Models.External.GoogleRoutes.Request;
using Catch_your_bus.Models.External.GoogleRoutes.Response;

namespace Catch_your_bus.Models.Services;

public class GoogleRoutesService(HttpClient httpClient, string apiKey) : IGoogleRoutesService
{
    private const string BaseUrl = "https://routes.googleapis.com/directions/v2:computeRoutes";

    // Request only the fields we actually use — keeps response small and latency low.
    private const string TransitFieldMask =
        "routes.duration," +
        "routes.distanceMeters," +
        "routes.legs.steps.travelMode," +
        "routes.legs.steps.staticDuration," +
        "routes.legs.steps.distanceMeters," +
        "routes.legs.steps.navigationInstruction," +
        "routes.legs.steps.startLocation," +
        "routes.legs.steps.endLocation," +
        "routes.legs.steps.transitDetails.stopDetails," +
        "routes.legs.steps.transitDetails.transitLine," +
        "routes.legs.steps.transitDetails.headsign," +
        "routes.legs.steps.transitDetails.stopCount";

    private const string WalkFieldMask =
        "routes.duration," +
        "routes.distanceMeters," +
        "routes.legs.steps.travelMode," +
        "routes.legs.steps.staticDuration," +
        "routes.legs.steps.distanceMeters," +
        "routes.legs.steps.navigationInstruction";

    public async Task<CalculatedRoute?> GetTransitRouteAsync(
        double startLat, double startLng,
        double endLat, double endLng,
        DateTimeOffset? departureTime = null)
    {
        var requestBody = BuildRequestBody(startLat, startLng, endLat, endLng, "TRANSIT", departureTime);
        var apiResponse = await SendRequestAsync(requestBody, TransitFieldMask);

        // If the query was for a time in the past, the user actually leaves NOW.
        var now = DateTimeOffset.Now;
        var actualRouteStartTime = (departureTime.HasValue && departureTime.Value > now)
            ? departureTime.Value
            : now;

        var route = GoogleRouteMapper.MapToCalculatedRoute(apiResponse, actualRouteStartTime);
        route?.Type = RouteType.StandardTransit;

        return route;
    }

    public async Task<CalculatedRoute?> GetWalkingRouteAsync(
        double startLat, double startLng,
        double endLat, double endLng)
    {
        var requestBody = BuildRequestBody(startLat, startLng, endLat, endLng, "WALK");
        var apiResponse = await SendRequestAsync(requestBody, WalkFieldMask);

        var route = GoogleRouteMapper.MapToCalculatedRoute(apiResponse, DateTimeOffset.Now);
        route?.Type = RouteType.PureWalking;

        return route;
    }

    private async Task<GoogleRoutesResponse?> SendRequestAsync(object requestBody, string fieldMask)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
        request.Content = JsonContent.Create(requestBody);
        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add("X-Goog-FieldMask", fieldMask);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoogleRoutesResponse>();
    }

    private static GoogleRoutesRequest BuildRequestBody(
        double startLat, double startLng,
        double endLat, double endLng,
        string travelMode,
        DateTimeOffset? departureTime = null)
    {
        return new GoogleRoutesRequest
        {
            Origin = new GoogleWaypoint
            {
                Location = new GoogleLocation
                    { LatLng = new GoogleLatLng { Latitude = startLat, Longitude = startLng } }
            },
            Destination = new GoogleWaypoint
            {
                Location = new GoogleLocation { LatLng = new GoogleLatLng { Latitude = endLat, Longitude = endLng } }
            },
            TravelMode = travelMode,
            DepartureTime = departureTime?.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture)
        };
    }
}