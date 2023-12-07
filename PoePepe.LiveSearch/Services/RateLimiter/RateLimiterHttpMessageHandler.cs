using System.Net;
using System.Net.Http.Headers;
using PoePepe.LiveSearch.Api.Trade;
using PoePepe.LiveSearch.Models.RateLimiter;
using Serilog;

namespace PoePepe.LiveSearch.Services.RateLimiter;

/// <summary>
/// Represents an HTTP message handler that applies rate limiting to outgoing requests.
/// </summary>
public class RateLimiterHttpMessageHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);
    private readonly RateLimiterState _rateLimiterState;

    public RateLimiterHttpMessageHandler(RateLimiterState rateLimiterState)
    {
        _rateLimiterState = rateLimiterState;
    }

    /// <summary>
    /// Sends an HTTP request asynchronously.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous send operation. The task result contains the HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var isRateLimited = IsRateLimited(request, out var policyName);
        if (isRateLimited)
        {
            if (_rateLimiterState.StateOfPolicies.TryGetValue(policyName, out var policy))
            {
                if (policy.IsWaitingForLimit)
                {
                    Log.Warning("Request {RequestUri} skipped due to waiting for a rate limiter",
                        request.RequestUri?.PathAndQuery);

                    return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                    {
                        ReasonPhrase =
                            $"Request {request.RequestUri?.PathAndQuery} skipped due to waiting for a rate limiter",
                        RequestMessage = request
                    };
                }
            }

            try
            {
                await SemaphoreSlim.WaitAsync(cancellationToken);
                return await CoreSendAsync(request, cancellationToken);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP request and handles rate limiting, if applicable.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An HTTP response message.
    /// If rate limiting is active, it returns a 429 Too Many Requests response if the limit is reached.
    /// </returns>
    private async Task<HttpResponseMessage> CoreSendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var isRateLimited = IsRateLimited(request, out var policyName);
        if (isRateLimited && _rateLimiterState.StateOfPolicies.TryGetValue(policyName, out var policy))
        {
            if (policy.IsWaitingForLimit)
            {
                Log.Warning("Request {RequestUri} skipped due to waiting for a rate limiter",
                    request.RequestUri?.PathAndQuery);

                return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    ReasonPhrase =
                        $"Request {request.RequestUri?.PathAndQuery} skipped due to waiting for a rate limiter",
                    RequestMessage = request
                };
            }

            var delay = GetDelayFromRule(policy);
            if (delay != TimeSpan.Zero)
            {
                await WaitForLimitAsync(policy, delay);
                policy.IsWaitingForLimit = false;
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (isRateLimited)
        {
            UpdateRateLimiterState(response);
        }

        return response;
    }

    /// <summary>
    /// Checks if the specified request is rate limited and retrieves the associated policy name.
    /// </summary>
    /// <param name="request">The HTTP request message to check for rate limiting.</param>
    /// <param name="policyName">The name of the rate limit policy associated with the request.</param>
    /// <returns>
    /// <c>true</c> if the request is rate limited; otherwise, <c>false</c>.
    /// </returns>
    private bool IsRateLimited(HttpRequestMessage request, out string policyName)
    {
        policyName = null;

        if (request.RequestUri is null)
        {
            return false;
        }

        if (request.RequestUri.PathAndQuery.StartsWith(PoeApiPath.ApiPathSearch))
        {
            policyName = RateLimitPolicy.SearchPolicy;
        }
        else if (request.RequestUri.PathAndQuery.StartsWith(PoeApiPath.ApiPathFetch))
        {
            policyName = RateLimitPolicy.FetchPolicy;
        }
        else if (request.RequestUri.PathAndQuery.StartsWith(PoeApiPath.ApiPathWhisper))
        {
            policyName = RateLimitPolicy.WhisperPolicy;
        }

        return policyName != null;
    }

    /// <summary>
    /// Waits for a specified delay if it's greater than zero.
    /// </summary>
    /// <param name="policy">The <see cref="RateLimiterPolicyState"/> which represents the rate limiter policy state.</param>
    /// <param name="delay">The delay to wait for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task WaitForLimitAsync(RateLimiterPolicyState policy, TimeSpan delay)
    {
        if (delay == TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        if (delay > TimeSpan.FromMilliseconds(1000))
        {
            policy.IsWaitingForLimit = true;
        }

        Log.Warning("Rules of policy {PolicyName} were broken. Need to wait a {Difference} ms", policy.PolicyName,
            delay.Milliseconds);

        return Task.Delay(delay);
    }

    /// <summary>
    /// Calculates the delay time based on the rate limiter policy state.
    /// </summary>
    /// <param name="policy">The rate limiter policy state.</param>
    /// <returns>The delay time as a TimeSpan.</returns>
    private TimeSpan GetDelayFromRule(RateLimiterPolicyState policy)
    {
        var now = DateTime.Now;

        if (policy.RetryAfter != 0)
        {
            var difference = now - policy.UpdatedAt;

            if (difference != TimeSpan.Zero)
            {
                Log.Warning("Need to wait a {Difference} ms", difference.Milliseconds);

                return difference;
            }
        }

        return GetDelayFromRule(now, policy);
    }

    /// <summary>
    /// Calculates the delay based on the given rate limiter policy state.
    /// </summary>
    /// <param name="now">The current date and time.</param>
    /// <param name="policy">The rate limiter policy state.</param>
    /// <returns>The maximum delay that needs to be imposed based on the policy rules.</returns>
    private TimeSpan GetDelayFromRule(DateTime now, RateLimiterPolicyState policy)
    {
        var maxTimeToWait = TimeSpan.Zero;

        foreach (var policyRules in policy.Rules)
        {
            var policyState = policy.States.First(x => x.Key == $"{policyRules.Key}-State").Value;
            for (var i = 0; i < policyRules.Value.Length; i++)
            {
                var state = policyState[i];
                var rule = policyRules.Value[i];

                var testedPeriod = rule.TestedPeriod;
                var ruleTime = now.AddSeconds(-testedPeriod);
                var requests = state.DateHistoryOfRequest.Where(x => x > ruleTime).ToArray();
                if (rule.CustomMaximumHits >= requests.Length)
                {
                    continue;
                }

                var oldestRequest = requests.First();
                var timeToWait = oldestRequest.AddSeconds(testedPeriod) - now;
                if (maxTimeToWait < timeToWait)
                {
                    maxTimeToWait = timeToWait;
                }
            }
        }

        return maxTimeToWait != TimeSpan.Zero ? maxTimeToWait : TimeSpan.Zero;
    }

    /// <summary>
    /// Parses the specified headers to extract rate limit related information.
    /// </summary>
    /// <param name="headers">The HTTP response headers containing rate limit information.</param>
    /// <returns>
    /// A tuple with three elements:
    /// - The rate limit policy name.
    /// - The retry-after value in seconds.
    /// - An array of rate limit rules.
    /// </returns>
    private (string policyName, string retryAfter, string[] rules) ParseHeaders(HttpResponseHeaders headers)
    {
        string policyName = null;
        string[] rules = null;
        var retryAfter = "0";
        foreach (var header in headers)
        {
            switch (header.Key)
            {
                case HeaderRateLimitKeys.RateLimitPolicy:
                    policyName = header.Value.First();
                    break;

                case HeaderRateLimitKeys.RateLimitRules:
                    rules = header.Value.First().Split(',');
                    break;

                case HeaderRateLimitKeys.RateLimitRetryAfter:
                    retryAfter = header.Value.First();
                    break;
            }
        }

        return (policyName, retryAfter, rules);
    }

    /// <summary>
    /// Updates the state of the rate limiter based on the response from the requested URL.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    private void UpdateRateLimiterState(HttpResponseMessage response)
    {
        var now = DateTime.Now;

        var (policyName, retryAfter, rules) = ParseHeaders(response.Headers);

        if (policyName is null)
        {
            Log.Error("Policy not found in request {RequestUri}", response.RequestMessage?.RequestUri?.PathAndQuery);
            return;
        }

        if (_rateLimiterState.StateOfPolicies.TryGetValue(policyName, out var policy))
        {
            policy = UpdatePolicyState(response.Headers, policy, rules);
        }
        else
        {
            policy = UpdatePolicyState(response.Headers, null, rules);
            policy.PolicyName = policyName;
            _rateLimiterState.StateOfPolicies.TryAdd(policyName, policy);
        }

        policy.RetryAfter = int.Parse(retryAfter!);
        policy.RuleNames = rules;
        policy.UpdatedAt = now;

        switch (response.StatusCode)
        {
            case HttpStatusCode.TooManyRequests:
                Log.Warning("Request {RequestUri} got status Too Many Requests (429)",
                    response.RequestMessage?.RequestUri?.PathAndQuery);
                break;

            case HttpStatusCode.OK:
                UpdateHistoryOfRequest(policy, now);
                break;

            default:
                Log.Warning("Request {RequestUri} got unexpected status {StatusCode}",
                    response.RequestMessage?.RequestUri?.PathAndQuery, response.StatusCode);
                break;
        }
    }

    /// <summary>
    /// Update the history of request for the given rate limiter policy state.
    /// </summary>
    /// <param name="policy">The rate limiter policy state to update the history of request for.</param>
    /// <param name="now">The current date and time.</param>
    private void UpdateHistoryOfRequest(RateLimiterPolicyState policy, DateTime now)
    {
        var historyMaximumRequestCount = 0;
        foreach (var rule in policy.Rules)
        {
            var lastRuleMaximumHits = rule.Value.Last().MaximumHits;
            if (historyMaximumRequestCount < lastRuleMaximumHits)
            {
                historyMaximumRequestCount = lastRuleMaximumHits;
            }
        }

        foreach (var policyStates in policy.States)
        {
            for (var index = 0; index < policyStates.Value.Length; index++)
            {
                var state = policyStates.Value[index];
                var testedPeriod = state.TestedPeriod;
                var ruleTime = now.AddSeconds(-testedPeriod);
                var requestCount = state.DateHistoryOfRequest.Count(x => x > ruleTime);
                var hitCount = state.CurrentHitCount - requestCount;

                var count = state.DateHistoryOfRequest.Count - historyMaximumRequestCount;
                for (var c = 0; c < count; c++)
                {
                    if (state.DateHistoryOfRequest.Count > historyMaximumRequestCount)
                    {
                        state.DateHistoryOfRequest.RemoveFirst();
                    }
                }

                for (var i = 0; i < hitCount; i++)
                {
                    state.DateHistoryOfRequest.AddLast(now);
                }
            }
        }
    }

    /// <summary>
    /// Updates the state of the rate limiter policy based on the provided response headers and rule names.
    /// </summary>
    /// <param name="headers">The HTTP response headers.</param>
    /// <param name="policyState">The current state of the rate limiter policy.</param>
    /// <param name="ruleNames">The names of the rate limiter rules to update.</param>
    /// <returns>The updated rate limiter policy state.</returns>
    private RateLimiterPolicyState UpdatePolicyState(HttpResponseHeaders headers, RateLimiterPolicyState policyState,
        string[] ruleNames)
    {
        policyState ??= new RateLimiterPolicyState();

        var rateLimitRules = ruleNames.Select(rule => string.Format(HeaderRateLimitKeys.RateLimitRulePrefix, rule))
            .ToArray();
        var rateLimitRuleStates = ruleNames
            .Select(state => string.Format(HeaderRateLimitKeys.RateLimitStatePrefix, state)).ToArray();

        foreach (var header in headers)
        {
            if (rateLimitRules.Contains(header.Key))
            {
                var rules = ParseFromHeader(header, (name, values) =>
                {
                    var maxCalculatedHits = (int)Math.Floor(int.Parse(values[0]) * 0.5);
                    return new RateLimitRule
                    {
                        Name = name,
                        MaximumHits = int.Parse(values[0]),
                        CustomMaximumHits = maxCalculatedHits == 0 ? 1 : maxCalculatedHits,
                        TestedPeriod = int.Parse(values[1]),
                        RestrictedTime = int.Parse(values[2])
                    };
                });

                policyState.Rules.AddOrUpdate(header.Key, rules, (_, _) => rules);
            }

            if (rateLimitRuleStates.Contains(header.Key))
            {
                var ruleStates = ParseFromHeader(header, (name, values) =>
                    new RateLimitState
                    {
                        Name = name,
                        CurrentHitCount = int.Parse(values[0]),
                        TestedPeriod = int.Parse(values[1]),
                        DurationOfTheRestriction = int.Parse(values[2])
                    });

                policyState.States.AddOrUpdate(header.Key, ruleStates, (_, _) => ruleStates);
            }
        }

        return policyState;
    }

    /// <summary>
    /// Parses and returns an array of objects of type T from the given header.
    /// </summary>
    /// <typeparam name="T">The type of objects to parse.</typeparam>
    /// <param name="header">The header containing the values to parse.</param>
    /// <param name="creator">A function that creates an object of type T given the header key and values.</param>
    /// <returns>An array of objects of type T parsed from the header.</returns>
    /// <remarks>
    /// The header value should be a comma-separated string, where each element is a key-value pair separated by a colon.
    /// The creator function is responsible for creating an object of type T by giving it the header key and values.
    /// </remarks>
    private T[] ParseFromHeader<T>(KeyValuePair<string, IEnumerable<string>> header, Func<string, string[], T> creator)
    {
        var valueOfObjects = header.Value.Single().Split(',');
        var objects = new T[valueOfObjects.Length];

        for (var index = 0; index < valueOfObjects.Length; index++)
        {
            var valueOfObject = valueOfObjects[index];
            var values = valueOfObject.Split(':');

            objects[index] = creator(header.Key, values);
        }

        return objects;
    }
}