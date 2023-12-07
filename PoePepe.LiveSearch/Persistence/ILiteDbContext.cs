using LiteDB;

namespace PoePepe.LiveSearch.Persistence;

/// <summary>
/// Represents a LiteDB database context.
/// </summary>
public interface ILiteDbContext
{
    /// <summary>
    /// Gets the LiteDatabase instance associated with the property.
    /// </summary>
    /// <value>
    /// The LiteDatabase instance.
    /// </value>
    public LiteDatabase Database { get; }
}