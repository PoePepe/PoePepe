using LiteDB;
using Microsoft.Extensions.Options;

namespace Poe.LiveSearch.Persistence;

public interface ILiteDbContext
{
    public LiteDatabase Database { get; }
}