namespace Poe.LiveSearch.Persistence;

public class LiteDbOptions
{
    public const string DefaultSection = "LiteDbOptions";

    public string DatabaseLocation { get; set; }

    public string SharedDatabaseLocation => $"Filename={DatabaseLocation}; Connection=Shared;";
}