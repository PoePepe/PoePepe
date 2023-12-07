using System.Text.Json.Serialization;

namespace PoePepe.LiveSearch.Api.Trade.Models;

public class SearchRequest
{
    [JsonPropertyName("query")] public Query Query { get; set; }

    [JsonPropertyName("sort")] public Sort Sort { get; set; }
}

public class Account
{
    [JsonPropertyName("input")] public string Input { get; set; }
}

public class Category
{
    [JsonPropertyName("option")] public string Option { get; set; }
}

public class Filters
{
    [JsonPropertyName("trade_filters")] public TradeFilters TradeFilters { get; set; }

    [JsonPropertyName("type_filters")] public TypeFilters TypeFilters { get; set; }

    [JsonPropertyName("account")] public Account Account { get; set; }

    [JsonPropertyName("category")] public Category Category { get; set; }
}

public class Query
{
    [JsonPropertyName("status")] public Status Status { get; set; }

    [JsonPropertyName("stats")] public Stat[] Stats { get; set; }

    [JsonPropertyName("filters")] public Filters Filters { get; set; }
}

public class Sort
{
    [JsonPropertyName("price")] public string Price { get; set; }
}

public class Stat
{
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("filters")] public object[] Filters { get; set; }

    [JsonPropertyName("disabled")] public bool Disabled { get; set; }
}

public class Status
{
    [JsonPropertyName("option")] public string Option { get; set; }
}

public class TradeFilters
{
    [JsonPropertyName("filters")] public Filters Filters { get; set; }

    [JsonPropertyName("disabled")] public bool Disabled { get; set; }
}

public class TypeFilters
{
    [JsonPropertyName("filters")] public Filters Filters { get; set; }
}