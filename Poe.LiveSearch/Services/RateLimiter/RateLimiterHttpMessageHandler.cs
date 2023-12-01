using System.Net;
using System.Net.Http.Headers;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Models.RateLimiter;
using Serilog;

namespace Poe.LiveSearch.Services.RateLimiter;

public class RateLimiterHttpMessageHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);
    private readonly RateLimiterState _rateLimiterState;

    public RateLimiterHttpMessageHandler(RateLimiterState rateLimiterState)
    {
        _rateLimiterState = rateLimiterState;
    }

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
                        RequestMessage = request,
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
                    RequestMessage = request,
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

    private (string policyName, string retryAfter, string[] rules) ParseHeaders(HttpResponseHeaders headers)
    {
        string policyName = null;
        string[] rules = null;
        string retryAfter = "0";
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
                var da = state.CurrentHitCount - requestCount;

                var count = state.DateHistoryOfRequest.Count - historyMaximumRequestCount;
                for (var c = 0; c < count; c++)
                {
                    if (state.DateHistoryOfRequest.Count > historyMaximumRequestCount)
                    {
                        state.DateHistoryOfRequest.RemoveFirst();
                    }
                }

                for (var i = 0; i < da; i++)
                {
                    state.DateHistoryOfRequest.AddLast(now);
                }
            }
        }
    }

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