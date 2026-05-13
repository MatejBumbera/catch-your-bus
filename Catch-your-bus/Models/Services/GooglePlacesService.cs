using System.Net.Http.Json;
using Catch_your_bus.Models.External.GooglePlaces;

namespace Catch_your_bus.Models.Services;

public class GooglePlacesService(HttpClient httpClient, string apiKey) : IGooglePlacesService
{
    private const string BaseUrl = "https://places.googleapis.com/v1/places";
    private const string AutocompleteUrl = $"{BaseUrl}:autocomplete";

    private const string AutocompleteFieldMask =
        "suggestions.placePrediction.placeId,suggestions.placePrediction.text.text";

    private const string DetailsFieldMask = "location";

    public async Task<List<PlacePrediction>> GetPredictionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        using var request = new HttpRequestMessage(HttpMethod.Post, AutocompleteUrl);
        request.Content = JsonContent.Create(new AutocompleteRequest { Input = query });
        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add("X-Goog-FieldMask", AutocompleteFieldMask);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AutocompleteResponse>();

        return result?.Suggestions?
            .Where(s => s.PlacePrediction is not null)
            .Select(s => s.PlacePrediction!)
            .ToList() ?? [];
    }

    public async Task<PlaceLocation?> GetPlaceCoordinatesAsync(string placeId)
    {
        if (string.IsNullOrWhiteSpace(placeId))
            return null;

        var url = $"{BaseUrl}/{placeId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add("X-Goog-FieldMask", DetailsFieldMask);

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PlaceDetailsResponse>();
        return result?.Location;
    }
}
