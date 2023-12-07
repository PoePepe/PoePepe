using PoePepe.LiveSearch.Api.Trade;
using PoePepe.LiveSearch.Services;

namespace PoePepe.LiveSearch.DependencyInjection;

public class SessionHeaderHttpMessageHandler : DelegatingHandler
{
    private const string CookieHeaderName = "Cookie";
    private readonly ServiceState _serviceState;

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
        if (requestPath.StartsWith(PoeApiPath.ApiPathWhisper) || requestPath.StartsWith(PoeApiPath.ApiPathFetch) || requestPath.StartsWith(PoeApiPath.ApiPathSearch) || requestPath.StartsWith(PoeApiPath.ApiPathLeague))
        {
            if (!request.Headers.Contains(CookieHeaderName))
            {
                request.Headers.Add(CookieHeaderName, _serviceState.Session);
            }
        }

        var httpResponseMessage = await base.SendAsync(request, cancellationToken);

        return httpResponseMessage;
    }
}