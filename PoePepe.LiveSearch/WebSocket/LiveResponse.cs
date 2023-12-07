using System.Text.Json.Serialization;

namespace PoePepe.LiveSearch.WebSocket;

public class LiveResponse
{
    [JsonPropertyName("new")]
    public string[] ItemIds { get; set; }
}