using System.Text.Json.Serialization;

namespace Catch_your_bus.Models.External.GooglePlaces.Request;

public class AutocompleteRequest
{
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;
}
