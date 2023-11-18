namespace Poe.LiveSearch.Api;

public class PoeApiOptions
{
    public const string DefaultSection = "PoeApiOptions";
    
    public string BaseApiAddress { get; set; }
    public string BaseWssAddress { get; set; }
    public string Session { get; set; }
    public string LeagueName { get; set; }
}