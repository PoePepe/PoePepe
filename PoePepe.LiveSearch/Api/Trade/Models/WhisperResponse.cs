using System.Text.Json.Serialization;

namespace PoePepe.LiveSearch.Api.Trade.Models;

public class WhisperResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}