using System.Collections.Concurrent;
using PoePepe.LiveSearch.Models.RateLimiter;

namespace PoePepe.LiveSearch.Services.RateLimiter;

public class RateLimiterState
{
    public ConcurrentDictionary<string, RateLimiterPolicyState> StateOfPolicies { get; set; } = new();
}

public class RateLimiterPolicyState
{
    public ConcurrentDictionary<string, RateLimitRule[]> Rules { get; set; } = new();
    public ConcurrentDictionary<string, RateLimitState[]> States { get; set; } = new();
    public int RetryAfter { get; set; }
    public string[] RuleNames { get; set; }
    public string PolicyName { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsWaitingForLimit { get; set; }
}