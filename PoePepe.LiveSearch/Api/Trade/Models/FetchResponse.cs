﻿using System.Text.Json.Serialization;
using PoePepe.LiveSearch.WebSocket;

namespace PoePepe.LiveSearch.Api.Trade.Models;

public class FetchResponse
{
    [JsonPropertyName("result")] public List<FetchResponseResult> Results { get; set; }
}

public class Extended
{
    [JsonPropertyName("base_defence_percentile")]
    public int? BaseDefencePercentile { get; set; }

    [JsonPropertyName("ev")]
    public double? Ev { get; set; }

    [JsonPropertyName("es")]
    public double? Es { get; set; }

    [JsonPropertyName("ar")]
    public double? Ar { get; set; }

    [JsonPropertyName("dps")]
    public double? Dps { get; set; }

    [JsonPropertyName("pdps")]
    public double? PDps { get; set; }

    [JsonPropertyName("ward")]
    public double? EDps { get; set; }

    public double? Ward { get; set; }
}

public class Item
{
    [JsonPropertyName("w")] public int Width { get; set; }

    [JsonPropertyName("icon")] public string Icon { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("typeLine")] public string TypeLine { get; set; }

    [JsonPropertyName("identified")] public bool Identified { get; set; }
    [JsonPropertyName("corrupted")] public bool Corrupted { get; set; }
    [JsonPropertyName("duplicated")] public bool Duplicated { get; set; }
    [JsonPropertyName("fractured")] public bool Fractured { get; set; }
    [JsonPropertyName("split")] public bool Split { get; set; }
    [JsonPropertyName("searing")] public bool Searing { get; set; }
    [JsonPropertyName("tangled")] public bool Tangled { get; set; }
    [JsonPropertyName("synthesised")] public bool Synthesised { get; set; }
    [JsonPropertyName("replica")] public bool Replica { get; set; }
    [JsonPropertyName("veiled")] public bool Veiled { get; set; }
    [JsonPropertyName("influences")] public Dictionary<string, bool> Influences { get; set; }

    [JsonPropertyName("ilvl")] public int ItemLevel { get; set; }

    [JsonPropertyName("properties")] public Property[] Properties { get; set; }

    [JsonPropertyName("requirements")] public Requirement[] Requirements { get; set; }

    [JsonPropertyName("fracturedMods")] public string[] FracturedMods { get; set; }
    [JsonPropertyName("enchantMods")] public string[] EnchantMods { get; set; }
    [JsonPropertyName("notableProperties")] public NotableProperty[] NotableProperties { get; set; }
    [JsonPropertyName("implicitMods")] public string[] ImplicitMods { get; set; }
    [JsonPropertyName("explicitMods")] public string[] ExplicitMods { get; set; }
    [JsonPropertyName("craftedMods")] public string[] CraftedMods { get; set; }

    [JsonPropertyName("logbookMods")] public LogbookMods[] LogbookMods { get; set; }

    [JsonPropertyName("descrText")] public string DescrText { get; set; }

    [JsonPropertyName("foilVariation")] public int FoilVariation { get; set; }
    [JsonPropertyName("frameType")] public int FrameType { get; set; }

    [JsonPropertyName("extended")] public Extended Extended { get; set; }

    [JsonPropertyName("sockets")]
    public List<Socket> Sockets { get; set; }
}

public class NotableProperty
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("values")] public object[][] Values { get; set; }
}

public class LogbookMods
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("faction")]
    public Faction Faction { get; set; }

    [JsonPropertyName("mods")]
    public string[] Mods { get; set; }
}

public class Faction
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class Socket
{
    [JsonPropertyName("group")] public int Group { get; set; }
    [JsonPropertyName("sColour")] public string Colour { get; set; }
}

public class Listing
{
    [JsonPropertyName("whisper")] public string WhisperMessage { get; set; }

    [JsonPropertyName("whisper_token")] public string WhisperToken { get; set; }

    [JsonPropertyName("price")] public Price Price { get; set; }
}

public class Online
{}

public class Price
{
    [JsonPropertyName("amount")] public double Amount { get; set; }

    [JsonPropertyName("currency")] public string Currency { get; set; }
}

public class Property
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("values")] public List<List<object>> Values { get; set; }
}

public class Requirement
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("values")] public List<List<object>> Values { get; set; }
}

public class FetchResponseResult
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("listing")] public Listing Listing { get; set; }

    [JsonPropertyName("item")] public Item Item { get; set; }

    [JsonIgnore] public IEnumerable<ItemLiveResponse> Orders { get; set; }

    [JsonIgnore] public long OrderId { get; set; }

    [JsonIgnore] public string OrderName { get; set; }

    [JsonIgnore] public bool IsWhispered { get; set; }
}