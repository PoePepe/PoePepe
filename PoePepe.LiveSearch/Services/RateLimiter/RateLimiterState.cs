using System.Collections.Concurrent;
using PoePepe.LiveSearch.Models.RateLimiter;

namespace PoePepe.LiveSearch.Services.RateLimiter;

/// <summary>
/// Represents the state of a rate limiter.
/// </summary>
public class RateLimiterState
{
    /// <summary>
    /// Gets or sets the state of the rate limiter policies.
    /// </summary>
    /// <value>
    /// The state of the rate limiter policies as a concurrent dictionary with string keys and RateLimiterPolicyState values.
    /// </value>
    public ConcurrentDictionary<string, RateLimiterPolicyState> StateOfPolicies { get; set; } = new();
}

/// <summary>
/// Represents the state of a rate limiter policy.
/// </summary>
public class RateLimiterPolicyState
{
    /// <summary>
    /// Gets or sets the collection of rate limit rules.
    /// </summary>
    /// <value>
    /// The dictionary where the key is a string representing the rate limit identifier and the value is an array of rate limit rules.
    /// </value>
    public ConcurrentDictionary<string, RateLimitRule[]> Rules { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of rate limit states by key.
    /// </summary>
    public ConcurrentDictionary<string, RateLimitState[]> States { get; set; } = new();

    /// <summary>
    /// Gets or sets the Retry-After property.
    /// </summary>
    /// <remarks>
    /// The Retry-After property indicates how long the client should wait before making a new request after receiving a 429 Too Many Requests response.
    /// It is represented as an integer value in seconds.
    /// A Retry-After value of zero means that the client can immediately make a new request.
    /// </remarks>
    /// <value>
    /// The number of seconds the client should wait before making a new request.
    /// </value>
    public int RetryAfter { get; set; }

    /// <summary>
    /// Gets or sets the array of rule names.
    /// </summary>
    /// <value>
    /// The array of rule names.
    /// </value>
    public string[] RuleNames { get; set; }

    /// Gets or sets the name of the policy.
    /// /
    public string PolicyName { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the property was last updated.
    /// </summary>
    /// <value>
    /// The date and time when the property was last updated.
    /// </value>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the object is waiting for a limit.
    /// </summary>
    /// <value>
    /// <c>true</c> if the object is waiting for a limit; otherwise, <c>false</c>.
    /// </value>
    public bool IsWaitingForLimit { get; set; }
}