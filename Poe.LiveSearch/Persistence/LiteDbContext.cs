using LiteDB;
using Microsoft.Extensions.Options;

namespace Poe.LiveSearch.Persistence;

public class LiteDbContext : ILiteDbContext
{
    public LiteDatabase Database { get; }

    public LiteDbContext(IOptions<LiteDbOptions> options)
    {
        Database = new LiteDatabase(options.Value.SharedDatabaseLocation);
    }
}