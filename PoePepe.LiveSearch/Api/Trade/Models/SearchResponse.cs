using System.Text.Json.Serialization;

namespace PoePepe.LiveSearch.Api.Trade.Models;

public class SearchResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("complexity")]
    public int Complexity { get; set; }

    [JsonPropertyName("result")]
    public string[] Result { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}