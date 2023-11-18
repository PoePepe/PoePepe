using System.Text.Json.Serialization;

namespace Poe.LiveSearch.WebSocket;

public class LiveResponse
{
    [JsonPropertyName("new")]
    public string[] ItemIds { get; set; }
}