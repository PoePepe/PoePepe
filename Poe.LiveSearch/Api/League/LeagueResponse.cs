using System.Text.Json.Serialization;

namespace Poe.LiveSearch.Api.League;

public class LeaguesResponse
{
    [JsonPropertyName("leagues")] public League[] Leagues { get; set; }
}

public class League
{
    [JsonPropertyName("id")] public string Name { get; set; }
}