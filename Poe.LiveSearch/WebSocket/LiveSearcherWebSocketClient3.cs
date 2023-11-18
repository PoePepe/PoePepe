using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using Poe.LiveSearch.Api;
using Poe.LiveSearch.Models;
using Serilog;

namespace Poe.LiveSearch.WebSocket;

public class LiveSearcherWebSocketClient3
{
    private readonly Order _order;
    private readonly PoeApiOptions _poeApiOptions;
    private readonly ClientWebSocket _clientWebSocket;
    private ChannelWriter<LiveResponse> _liveResponseChannelWriter;
    private bool _isConnected;

    private LiveSearcherWebSocketClient3(Order order, PoeApiOptions poeApiOptions)
    {
        _order = order;
        _poeApiOptions = poeApiOptions;

        _clientWebSocket = new ClientWebSocket();
    }

    public static async Task<LiveSearcherWebSocketClient3> CreateAndConnectAsync(Order order, PoeApiOptions poeApiOptions,
        CancellationToken token = default)
    {
        var client = new LiveSearcherWebSocketClient3(order, poeApiOptions);
        client.SetHeaders();

        await client.ConnectAsync(token);

        return client;
    }

    public async Task StartReceiveAsync(ChannelWriter<LiveResponse> channelResponseWriter, CancellationToken token)
    {
        if (!_isConnected)
        {
            Log.Warning("Receiving data from web socket for order {OrderName} not started", _order.Name);

            return;
        }

        Log.Information("Started receiving data from web socket for order {OrderName}", _order.Name);

        try
        {
            _liveResponseChannelWriter = channelResponseWriter;

            var buffer = new byte[8192];

            while (!token.IsCancellationRequested)
            {
                var outputStream = new MemoryStream(8192);

                WebSocketReceiveResult receiveResult;
                do
                {
                    receiveResult = await _clientWebSocket.ReceiveAsync(buffer, token);
                    if (receiveResult.MessageType != WebSocketMessageType.Close)
                    {
                        outputStream.Write(buffer, 0, receiveResult.Count);
                    }
                } while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                outputStream.Position = 0;

                await ProcessReceivedMessageAsync(outputStream, token);
            }
        }
        catch (TaskCanceledException e)
        {
            Log.Information("Receiving data from web socket for order {OrderName} has been stopped", _order.Name);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error receiving data from web socket for order {OrderName}", _order.Name);
        }
    }

    private async Task ConnectAsync(CancellationToken token = default)
    {
        var uri = new Uri($"{_poeApiOptions.BaseWssAddress}/{_poeApiOptions.LeagueName}/{_order.QueryHash}");
        try
        {
            await _clientWebSocket.ConnectAsync(uri, token);
            _isConnected = true;
            Log.Information("Connected to web socket for order {OrderName}", _order.Name);
        }
        catch (WebSocketException e) when(e.WebSocketErrorCode == WebSocketError.NotAWebSocket)
        {
            Log.Error("The server returned status code '404' when status code '101' was expected. The URI {Uri} not a correct websocket address", uri);
        }
        catch (TaskCanceledException e)
        {
            Log.Information("Receiving data from web socket for order {OrderName} has been stopped", _order.Name);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error connecting to web socket for order {OrderName}", _order.Name);
        }
    }

    private void SetHeaders()
    {
        _clientWebSocket.Options.SetRequestHeader("Cookie", _poeApiOptions.Session);
        _clientWebSocket.Options.SetRequestHeader("Origin", "https://www.pathofexile.com");
        _clientWebSocket.Options.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
    }

    private async Task ProcessReceivedMessageAsync(Stream messageStream, CancellationToken cancellationToken)
    {
        var message =
            await JsonSerializer.DeserializeAsync<LiveResponse>(messageStream, cancellationToken: cancellationToken);

        if (message?.ItemIds is not null)
        {
            // message.OrderId = _order.Id;
            // message.OrderName = _order.Name;
            await _liveResponseChannelWriter.WriteAsync(message, cancellationToken);

            Log.Debug("Received item of order {OrderName}", _order.Name);
        }

        await messageStream.DisposeAsync();
    }
}