using Microsoft.Extensions.Options;
using Poe.LiveSearch.Api;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Services;

namespace Poe.LiveSearch.DependencyInjection;

public class SessionHeaderHttpMessageHandler : DelegatingHandler
{
    private readonly ServiceState _serviceState;
    private const string CookieHeaderName = "Cookie";

    public SessionHeaderHttpMessageHandler(ServiceState serviceState)
    {
        _serviceState = serviceState;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestPath = request.RequestUri.PathAndQuery;
        if (requestPath.StartsWith(PoeApiPath.ApiPathWhisper) || requestPath.StartsWith(PoeApiPath.ApiPathFetch) || requestPath.StartsWith(PoeApiPath.ApiPathLeague))
        {
            request.Headers.Add(CookieHeaderName, _serviceState.Session);
        }

        var httpResponseMessage = await base.SendAsync(request, cancellationToken);

        return httpResponseMessage;
    }
}