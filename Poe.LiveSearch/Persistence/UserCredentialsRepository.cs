using LiteDB;
using Poe.LiveSearch.Models;

namespace Poe.LiveSearch.Persistence;

public class UserCredentialsRepository : IUserCredentialsRepository
{
    // private readonly ILiteCollection<UserCredentials> _collection;

    private UserCredentials _cached;

    public UserCredentialsRepository()
    {
        // _collection = liteDbContext.Database.GetCollection<UserCredentials>();
        // _cached = _collection.Query().FirstOrDefault();
    }

    public UserCredentials Get()
    {
        // _cached ??= _collection.Query().FirstOrDefault();
        return _cached;
    }
    
    public UserCredentials Create(UserCredentials credentials)
    {
        // _collection.DeleteAll();

        // _collection.Insert(credentials);

        _cached = credentials;

        return credentials;
    }
}