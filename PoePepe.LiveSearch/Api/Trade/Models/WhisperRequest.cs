using System.Text.Json.Serialization;

namespace PoePepe.LiveSearch.Api.Trade.Models;

public class WhisperRequest
{
    public WhisperRequest(string whisperToken)
    {
        WhisperToken = whisperToken;
    }

    [JsonPropertyName("token")] public string WhisperToken { get; set; }
}