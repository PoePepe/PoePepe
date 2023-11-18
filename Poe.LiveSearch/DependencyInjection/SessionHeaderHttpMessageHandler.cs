using Microsoft.Extensions.Options;
using Poe.LiveSearch.Api;
using Poe.LiveSearch.Api.Trade;

namespace Poe.LiveSearch.DependencyInjection;

public class SessionHeaderHttpMessageHandler : DelegatingHandler
{
    private readonly PoeApiOptions _poeApiOptions;
    private const string CookieHeaderName = "Cookie";

    public SessionHeaderHttpMessageHandler(IOptions<PoeApiOptions> poeApiOptions)
    {
        _poeApiOptions = poeApiOptions.Value;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri.PathAndQuery.StartsWith(PoeApiPath.ApiPathWhisper) || request.RequestUri.PathAndQuery.StartsWith(PoeApiPath.ApiPathFetch))
        {
            request.Headers.Add(CookieHeaderName, _poeApiOptions.Session);
        }

        var httpResponseMessage = await base.SendAsync(request, cancellationToken);

        return httpResponseMessage;
    }
}