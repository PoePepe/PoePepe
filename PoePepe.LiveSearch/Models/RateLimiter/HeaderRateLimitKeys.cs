namespace PoePepe.LiveSearch.Models.RateLimiter;

public static class HeaderRateLimitKeys
{
    public const string RateLimitPolicy = "X-Rate-Limit-Policy";
    public const string RateLimitRules = "X-Rate-Limit-Rules";
    public const string RateLimitRetryAfter = "Retry-After";
    public const string RateLimitRulePrefix = "X-Rate-Limit-{0}";
    public const string RateLimitStatePrefix = "X-Rate-Limit-{0}-State";

    // public const string RateLimitStatePrefix3 = "{0}-State";
}