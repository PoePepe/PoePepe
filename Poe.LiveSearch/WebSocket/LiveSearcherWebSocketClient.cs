using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using Poe.LiveSearch.Api;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Services;
using Serilog;

namespace Poe.LiveSearch.WebSocket;

public class LiveSearcherWebSocketClient : IDisposable
{
    private readonly Order _order;
    private readonly PoeApiOptions _poeApiOptions;
    private readonly ServiceState _serviceState;
    private readonly ClientWebSocket _clientWebSocket;
    private readonly ChannelWriter<ItemLiveResponse> _liveResponseChannelWriter;
    private readonly ChannelWriter<OrderError> _orderErrorChannelWriter;
    private bool _isConnected;

    public EventHandler OnConnected;
    public EventHandler OnConnectionFailed;
    public EventHandler<OrderProcessingFailedEventArgs> OnProcessingFailed;

    public LiveSearcherWebSocketClient(Order order, PoeApiOptions poeApiOptions, ServiceState serviceState)
    {
        _poeApiOptions = poeApiOptions;
        _serviceState = serviceState;
        _liveResponseChannelWriter = serviceState.LiveSearchChannel.Writer;
        _orderErrorChannelWriter = serviceState.OrderErrorChannel.Writer;

        _order = order;
        _clientWebSocket = new ClientWebSocket();

        SetHeaders();
    }

    public Task<bool> TryConnectAsync(CancellationToken token = default)
    {
        return ConnectCoreAsync(token);
    }

    public async Task<bool> ConnectAsync(CancellationToken token = default)
    {
        for (var retryCount = 1; retryCount < 4; retryCount++)
        {
            var isConnected = await ConnectCoreAsync(token);
            if (isConnected)
            {
                return true;
            }

            if (retryCount == 3)
            {
                return false;
            }

            var timeout = Math.Pow(3, retryCount);

            await Task.Delay(TimeSpan.FromSeconds(timeout), token);
        }

        return false;
    }

    public async Task StartReceiveAsync(CancellationToken token)
    {
        if (!_isConnected)
        {
            Log.Warning("Receiving data from web socket for order {OrderName} not started", _order.Name);

            return;
        }

        Log.Information("Started receiving data from web socket for order {OrderName}", _order.Name);

        try
        {
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

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                outputStream.Position = 0;

                await ProcessReceivedMessageAsync(outputStream, token);
            }
        }
        catch (TaskCanceledException)
        {
            Log.Information("Receiving data from web socket for order {OrderName} has been stopped", _order.Name);
        }
        catch (Exception e)
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, e.Message, OrderErrorType.Process));

            Log.Error(e, "Error receiving data from web socket for order {OrderName}", _order.Name);

            OnProcessingFailed?.Invoke(this, new OrderProcessingFailedEventArgs(_order.Id));
        }
    }

    private void SetHeaders()
    {
        _clientWebSocket.Options.SetRequestHeader("Cookie", _serviceState.Session);
        _clientWebSocket.Options.SetRequestHeader("Origin", "https://www.pathofexile.com");
        _clientWebSocket.Options.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
    }

    private async Task<bool> ConnectCoreAsync(CancellationToken token = default)
    {
        var uri = new Uri($"{_poeApiOptions.BaseWssAddress}/{_serviceState.LeagueName}/{_order.QueryHash}");
        try
        {
            await _clientWebSocket.ConnectAsync(uri, token);

            _isConnected = true;

            Log.Information("Successfully connected to web socket for order {OrderName} on {LeagueName}", _order.Name, _serviceState.LeagueName);

            return true;
        }
        catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.NotAWebSocket &&
                                           e.Message.Contains("404"))
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, "Invalid link"));

            Log.Error(e, "The URI {Uri} not a correct websocket address", uri);

            return true;
        }
        catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.NotAWebSocket &&
                                           e.Message.Contains("429"))
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, "Rate limit exceed. Wait a little time."));

            Log.Error(e, "The URI {Uri} rate limit exceed. Wait a little time", uri);
        }
        catch (WebSocketException e)
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, $"Connection error: {e.Message}"));

            Log.Error(e, "The URI {Uri} occured error {Error}", uri, e.Message);
        }
        catch (TaskCanceledException)
        {
            Log.Information("Connecting to web socket for order {OrderName} was cancelled", _order.Name);

            return true;
        }
        catch (Exception e)
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, e.Message));

            Log.Error(e, "Error connecting to web socket for order {OrderName}", _order.Name);
        }

        return false;
    }

    private async Task ProcessReceivedMessageAsync(Stream messageStream, CancellationToken cancellationToken)
    {
        var message =
            await JsonSerializer.DeserializeAsync<LiveResponse>(messageStream, cancellationToken: cancellationToken);

        if (message?.ItemIds is not null)
        {
            foreach (var itemId in message.ItemIds)
            {
                var itemLiveResponse = new ItemLiveResponse(itemId, _order.Id, _order.Name);
                await _liveResponseChannelWriter.WriteAsync(itemLiveResponse, cancellationToken);
            }

            Log.Debug("Received item of order {OrderName}", _order.Name);
        }

        await messageStream.DisposeAsync();
    }

    public void Dispose()
    {
        _clientWebSocket?.Dispose();
    }
}