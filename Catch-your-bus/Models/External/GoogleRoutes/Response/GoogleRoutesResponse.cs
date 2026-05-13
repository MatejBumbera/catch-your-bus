using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GoogleRoutes.Response;

/// <summary>
/// Reponse is documented at: https://developers.google.com/maps/documentation/routes/reference/rest/v2/TopLevel/computeRoutes#Route
/// </summary>
public class GoogleRoutesResponse
{
    [JsonPropertyName("routes")]
    public List<GoogleRoute>? Routes { get; set; }
}
