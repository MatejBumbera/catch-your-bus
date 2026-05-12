using System.Globalization;
using System.Net.Http.Json;
using Catch_your_bus.Models.Entities;
using Catch_your_bus.Models.External.GoogleRoutes;

namespace Catch_your_bus.Models.Services;

public class GoogleRoutesService(HttpClient httpClient, string apiKey) : IGoogleRoutesService
{
    private const string BaseUrl = "https://routes.googleapis.com/directions/v2:computeRoutes";

    // Request only the fields we actually use — keeps response small and latency low.
    private const string FieldMask =
        "routes.duration," +
        "routes.distanceMeters," +
        "routes.legs.steps.travelMode," +
        "routes.legs.steps.staticDuration," +
        "routes.legs.steps.distanceMeters," +
        "routes.legs.steps.navigationInstruction," +
        "routes.legs.steps.transitDetails.stopDetails," +
        "routes.legs.steps.transitDetails.transitLine," +
        "routes.legs.steps.transitDetails.stopCount";

    public async Task<CalculatedRoute?> GetTransitRouteAsync(
        double startLat, double startLng,
        double endLat, double endLng)
    {
        var requestBody = BuildRequestBody(startLat, startLng, endLat, endLng);

        using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
        request.Content = JsonContent.Create(requestBody);
        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add("X-Goog-FieldMask", FieldMask);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content.ReadFromJsonAsync<GoogleRoutesResponse>();
        return GoogleRouteMapper.MapToCalculatedRoute(apiResponse);
    }

    private static object BuildRequestBody(
        double startLat, double startLng,
        double endLat, double endLng)
    {
        return new
        {
            origin = new
            {
                location = new
                {
                    latLng = new
                    {
                        latitude = startLat,
                        longitude = startLng
                    }
                }
            },
            destination = new
            {
                location = new
                {
                    latLng = new
                    {
                        latitude = endLat,
                        longitude = endLng
                    }
                }
            },
            travelMode = "TRANSIT"
        };
    }
}