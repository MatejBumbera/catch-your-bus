using Catch_your_bus.Models.External.GooglePlaces;

namespace Catch_your_bus.Models.Services;

/// <summary>
/// Api is documented at: https://developers.google.com/maps/documentation/places/web-service/op-overview
/// </summary>
public interface IGooglePlacesService
{
    /// <summary>
    /// Searches for place predictions using the Google Places Autocomplete API (New, v1).
    /// </summary>
    Task<List<PlacePrediction>> GetPredictionsAsync(string query);

    /// <summary>
    /// Fetches the lat/lng coordinates for a given placeId via the Place Details API.
    /// Returns null if the place has no location data.
    /// </summary>
    Task<PlaceLocation?> GetPlaceCoordinatesAsync(string placeId);
}
