using System.Net;
using System.Net.Http.Headers;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Models.RateLimiter;
using Serilog;

namespace Poe.LiveSearch.Services.RateLimiter;

public class RateLimiterHttpMessageHandler : DelegatingHandler
{
    private readonly RateLimiterState _rateLimiterState;

    public RateLimiterHttpMessageHandler(RateLimiterState rateLimiterState)
    {
        _rateLimiterState = rateLimiterState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var policyExists = TryGetPolicy(request, out var policy);
        if (policyExists)
        {
            if (policy.IsWaitingForLimit)
            {
                Log.Warning("Request {RequestUri} skipped due to waiting for a rate limiter", request.RequestUri?.PathAndQuery);

                return new HttpResponseMessage(HttpStatusCode.TooManyRequests); 
            }
            
            var delay = GetDelayFromRule(policy);
            await WaitForLimitAsync(policy, delay);
            policy.IsWaitingForLimit = false;
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (policyExists)
        {
            UpdateRateLimiterState(response);
        }

        return response;
    }

    private bool TryGetPolicy(HttpRequestMessage request, out RateLimiterPolicyState policy)
    {
        string policyName = null;
        policy = null;

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
        
        return policyName != null && _rateLimiterState.StateOfPolicies.TryGetValue(policyName, out policy);
    }

    private Task WaitForLimitAsync(RateLimiterPolicyState policy, TimeSpan delay)
    {
        if (delay == TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        if (delay > TimeSpan.FromSeconds(1))
        {
            policy.IsWaitingForLimit = true;
        }

        Log.Warning("Rules of policy {PolicyName} were broken. Need to wait a {Difference} ms", policy.PolicyName, delay.Milliseconds);

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

        var rules = policy.Rules.SelectMany(x => x.Value);
        return GetDelayFromRule(now, policy, rules);
    }

    // private Task CheckRateLimiterState(string policyName)
    // {
    //     var now = DateTime.Now;
    //
    //     if (!_rateLimiterState.StateOfPolicies.TryGetValue(policyName, out var policy))
    //     {
    //         return Task.CompletedTask;
    //     }
    //
    //     if (policy.RetryAfter != 0)
    //     {
    //         var difference = DateTime.Now - policy.UpdatedAt;
    //         Log.Warning("Rules of policy {PolicyName} were broken. Need to wait a {Difference} ms", policyName, difference.Milliseconds);
    //         return Task.Delay(difference);
    //     }
    //
    //     var rules = policy.Rules.SelectMany(x => x.Value);
    //     return CheckRuleAndWaitAsync(now, policy, rules);
    // }
    
    private TimeSpan GetDelayFromRule(DateTime now, RateLimiterPolicyState policy,
        IEnumerable<RateLimitRule> rules)
    {
        var maxTimeToWait = TimeSpan.Zero;
        foreach (var rule in rules)
        {
            var ruleTime = now.AddSeconds(-rule.TestedPeriod);

            var requests = policy.DateHistoryOfRequest.Where(x => x > ruleTime).ToArray();
            var requestsCount = requests.Length;
            if (rule.MaximumHits >= requestsCount)
            {
                continue;
            }

            var oldestRequest = requests[^1];
            var preOldestRequest = requests[^2];
            var timeToWait = preOldestRequest - oldestRequest;
            if (maxTimeToWait < timeToWait)
            {
                maxTimeToWait = timeToWait;
            }
        }

        return maxTimeToWait != TimeSpan.Zero ? maxTimeToWait : TimeSpan.Zero;
    }

    // private Task CheckRuleAndWaitAsync(DateTime now, RateLimiterPolicyState policy,
    //     IEnumerable<RateLimitRule> rules)
    // {
    //     var maxTimeToWait = TimeSpan.Zero;
    //     foreach (var rule in rules)
    //     {
    //         var ruleTime = now.AddSeconds(-rule.TestedPeriod);
    //
    //         var requests = policy.DateHistoryOfRequest.Where(x => x > ruleTime).ToArray();
    //         var requestsCount = requests.Length;
    //         if (rule.MaximumHits < requestsCount)
    //         {
    //             var oldestRequest = requests[^1];
    //             var preOldestRequest = requests[^2];
    //             var timeToWait = preOldestRequest - oldestRequest;
    //             if (maxTimeToWait < timeToWait)
    //             {
    //                 maxTimeToWait = timeToWait;
    //             }
    //         }
    //     }
    //
    //     if (maxTimeToWait != TimeSpan.Zero)
    //     {
    //         Log.Warning("Limits of policy {PolicyName} has been reached. Need to wait a {Difference} ms", policy.PolicyName, maxTimeToWait.Milliseconds);
    //
    //         return Task.Delay(maxTimeToWait);
    //     }
    //     
    //     return Task.CompletedTask;
    // }

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
                Log.Warning("Request {RequestUri} got status Too Many Requests (429)", response.RequestMessage?.RequestUri?.PathAndQuery);
                break;
            
            case HttpStatusCode.OK:
                policy.DateHistoryOfRequest = UpdateHistoryOfRequest(policy, now);
                break;
            
            default:
                Log.Warning("Request {RequestUri} got unexpected status {StatusCode}", response.RequestMessage?.RequestUri?.PathAndQuery, response.StatusCode);
                break;
        }
    }

    private LinkedList<DateTime> UpdateHistoryOfRequest(RateLimiterPolicyState policy, DateTime now)
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

        if (policy.DateHistoryOfRequest.Count >= historyMaximumRequestCount)
        {
            policy.DateHistoryOfRequest.RemoveFirst();
        }

        policy.DateHistoryOfRequest.AddLast(now);

        return policy.DateHistoryOfRequest;
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
            // if (rateLimitRules.Contains(header.Key))
            // {
            //     var valueOfRules = header.Value.SingleOrDefault().Split(',');
            //     var rules = new RateLimitRule[valueOfRules.Length];
            //
            //     for (var index = 0; index < valueOfRules.Length; index++)
            //     {
            //         var valueOfRule = valueOfRules[index];
            //         var values = valueOfRule.Split(':');
            //
            //         var rule = new RateLimitRule
            //         {
            //             Name = header.Key,
            //             MaximumHits = int.Parse(values[0]),
            //             TestedPeriod = int.Parse(values[1]),
            //             RestrictedTime = int.Parse(values[2])
            //         };
            //
            //         rules[index] = rule;
            //     }
            //     
            //     policyState.Rules.AddOrUpdate(header.Key, rules, (_, _) => rules);
            //
            // }

            if (rateLimitRules.Contains(header.Key))
            {
                var rules = ParseFromHeader(header, (name, values) =>
                    new RateLimitRule
                    {
                        Name = name,
                        MaximumHits = int.Parse(values[0]),
                        TestedPeriod = int.Parse(values[1]),
                        RestrictedTime = int.Parse(values[2])
                    });

                policyState.Rules.AddOrUpdate(header.Key, rules, (_, _) => rules);
            }

            // if (rateLimitRuleStates.Contains(header.Key))
            // {
            //     var valueOfRuleStates = header.Value.SingleOrDefault().Split(',');
            //     var ruleStates = new RateLimitState[valueOfRuleStates.Length];
            //
            //     for (var index = 0; index < valueOfRuleStates.Length; index++)
            //     {
            //         var valueOfRuleState = valueOfRuleStates[index];
            //         var values = valueOfRuleState.Split(':');
            //         
            //         var ruleState = new RateLimitState
            //         {
            //             Name = header.Key,
            //             CurrentHitCount = int.Parse(values[0]),
            //             TestedPeriod = int.Parse(values[1]),
            //             DurationOfTheRestriction = int.Parse(values[2])
            //         };
            //
            //         ruleStates[index] = ruleState;
            //     }
            //
            //     policyState.States.AddOrUpdate(header.Key, ruleStates, (_, _) => ruleStates);
            // }

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