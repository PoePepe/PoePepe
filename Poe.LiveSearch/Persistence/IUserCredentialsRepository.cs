using Poe.LiveSearch.Models;

namespace Poe.LiveSearch.Persistence;

public interface IUserCredentialsRepository
{
    UserCredentials Get();
    UserCredentials Create(UserCredentials credentials);
}