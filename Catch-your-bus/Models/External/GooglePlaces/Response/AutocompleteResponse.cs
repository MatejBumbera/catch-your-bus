using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GooglePlaces.Response;

public class AutocompleteResponse
{
    [JsonPropertyName("suggestions")]
    public List<Suggestion>? Suggestions { get; set; }
}
