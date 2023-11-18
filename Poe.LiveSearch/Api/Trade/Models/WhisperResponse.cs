using System.Text.Json.Serialization;

namespace Poe.LiveSearch.Api.Trade.Models;

public class WhisperResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}