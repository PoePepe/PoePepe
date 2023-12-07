using LiteDB;

namespace PoePepe.LiveSearch.Persistence;

public interface ILiteDbContext
{
    public LiteDatabase Database { get; }
}