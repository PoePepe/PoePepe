using LiteDB;
using Microsoft.Extensions.Options;

namespace PoePepe.LiveSearch.Persistence;

public class LiteDbContext : ILiteDbContext
{
    public LiteDbContext(IOptions<LiteDbOptions> options)
    {
        Database = new LiteDatabase(options.Value.SharedDatabaseLocation);
    }

    public LiteDatabase Database { get; }
}