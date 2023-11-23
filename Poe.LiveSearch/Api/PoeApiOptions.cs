namespace Poe.LiveSearch.Api;

public class PoeApiOptions
{
    public const string DefaultSection = "PoeApiOptions";
    
    public string BaseInternalApiAddress { get; set; }
    public string BaseExternalApiAddress { get; set; }
    public string BaseWssAddress { get; set; }
}