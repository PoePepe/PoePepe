using System.Text.Json.Serialization;

namespace PoePepe.LiveSearch.WebSocket;

/// <summary>
/// Represents a response from a live event.
/// </summary>
public class LiveResponse
{
    /// <summary>
    /// Gets or sets the array of item IDs.
    /// </summary>
    /// <value>
    /// The array of item IDs.
    /// </value>
    [JsonPropertyName("new")]
    public string[] ItemIds { get; set; }
}