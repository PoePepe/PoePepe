using LiteDB;
using Microsoft.Extensions.Options;

namespace PoePepe.LiveSearch.Persistence;

/// <summary>
/// The LiteDbContext class is responsible for managing the LiteDB context.
/// </summary>
public class LiteDbContext : ILiteDbContext
{
    /// <summary>
    /// Represents a database context for LiteDB.
    /// </summary>
    public LiteDbContext(IOptions<LiteDbOptions> options)
    {
        Database = new LiteDatabase(options.Value.SharedDatabaseLocation);
    }

    /// <summary>
    /// Gets the LiteDatabase instance associated with the property.
    /// </summary>
    /// <remarks>
    /// This property provides access to the LiteDatabase instance used by the object.
    /// The LiteDatabase class is a lightweight embedded NoSQL database solution for .NET.
    /// It allows you to easily store, retrieve, and query data using a document-oriented approach.
    /// </remarks>
    /// <returns>
    /// The LiteDatabase instance associated with the property.
    /// </returns>
    public LiteDatabase Database { get; }
}