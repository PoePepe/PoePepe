namespace Poe.LiveSearch.Models;

public class UserCredentials
{
    public UserCredentials(string sessionId)
    {
        SessionId = sessionId;
    }

    public string SessionId { get; set; }
}