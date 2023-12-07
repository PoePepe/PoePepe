namespace PoePepe.LiveSearch.Persistence;

/// <summary>
/// Represents the options for LiteDB configuration.
/// </summary>
public class LiteDbOptions
{
    /// <summary>
    /// The default section for LiteDbOptions in the configuration file.
    /// </summary>
    public const string DefaultSection = "LiteDbOptions";

    /// <summary>
    /// Gets or sets the location of the database.
    /// </summary>
    /// <value>
    /// The location of the database.
    /// </value>
    public string DatabaseLocation { get; set; }

    /// <summary>
    /// Gets the location of the shared database.
    /// </summary>
    /// <remarks>
    /// This property returns the location of the shared database as a connection string.
    /// The connection string is constructed using the <see cref="DatabaseLocation"/> property
    /// and the shared database connection option.
    /// </remarks>
    /// <value>
    /// A connection string representing the address of the shared database.
    /// </value>
    public string SharedDatabaseLocation => $"Filename={DatabaseLocation}; Connection=Shared;";
}