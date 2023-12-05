using System.Text.Json.Serialization;

namespace Poe.UIW.Models;

public class BetterTradingItems
{
    [JsonPropertyName("trs")] public BetterTradingItem[] Items { get; set; }
}

public class BetterTradingItem
{
    [JsonPropertyName("tit")] public string Title { get; set; }
    [JsonPropertyName("loc")] public string Query { get; set; }
}