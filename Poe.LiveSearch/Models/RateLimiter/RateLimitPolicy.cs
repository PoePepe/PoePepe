namespace Poe.LiveSearch.Models.RateLimiter;

public static class RateLimitPolicy
{
    public const string SearchPolicy = "trade-search-request-limit";
    public const string FetchPolicy = "trade-fetch-request-limit";
    public const string WhisperPolicy = "trade-whisper-request-limit";
}