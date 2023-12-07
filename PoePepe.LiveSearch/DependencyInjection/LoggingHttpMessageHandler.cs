using Serilog;

namespace PoePepe.LiveSearch.DependencyInjection;

public class LoggingHttpMessageHandler : DelegatingHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpResponseMessage = await base.SendAsync(request, cancellationToken);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            Log.Debug("Successfully request {RequestUri} with status code {StatusCode}",
                httpResponseMessage.RequestMessage?.RequestUri, httpResponseMessage.StatusCode);
        }
        else
        {
            Log.Warning("Unsuccessfully request {RequestUri} with status code {StatusCode} and message {ErrorMessage}",
                httpResponseMessage.RequestMessage?.RequestUri, httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
        }

        return httpResponseMessage;
    }
}