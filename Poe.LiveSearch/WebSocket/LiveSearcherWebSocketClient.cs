using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using Poe.LiveSearch.Api;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Services;
using Serilog;

namespace Poe.LiveSearch.WebSocket;

public class LiveSearcherWebSocketClient
{
    private Order _order;
    private readonly PoeApiOptions _poeApiOptions;
    private readonly ServiceState _serviceState;
    private ClientWebSocket _clientWebSocket;
    private readonly ChannelWriter<ItemLiveResponse> _liveResponseChannelWriter;
    private readonly ChannelWriter<OrderError> _orderErrorChannelWriter;
    private bool _isConnected;

    public EventHandler OnConnected;
    public EventHandler OnConnectionFailed;

    public LiveSearcherWebSocketClient(PoeApiOptions poeApiOptions, ServiceState serviceState)
    {
        _poeApiOptions = poeApiOptions;
        _serviceState = serviceState;
        _liveResponseChannelWriter = serviceState.LiveSearchChannel.Writer;
        _orderErrorChannelWriter = serviceState.OrderErrorChannel.Writer;
    }

    public async Task ConnectAsync(Order order, CancellationToken token = default)
    {
        _order = order;
        _clientWebSocket = new ClientWebSocket();

        SetHeaders();

        await ConnectAsync(token);
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
        }
    }

    private void SetHeaders()
    {
        _clientWebSocket.Options.SetRequestHeader("Cookie", _serviceState.Session);
        _clientWebSocket.Options.SetRequestHeader("Origin", "https://www.pathofexile.com");
        _clientWebSocket.Options.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
    }

    private async Task ConnectAsync(CancellationToken token = default)
    {
        var uri = new Uri($"{_poeApiOptions.BaseWssAddress}/{_order.LeagueName}/{_order.QueryHash}");
        try
        {
            await _clientWebSocket.ConnectAsync(uri, token);
            
            _isConnected = true;
            OnConnected?.Invoke(this, EventArgs.Empty);

            Log.Information("Connected to web socket for order {OrderName}", _order.Name);

            return;
        }
        catch (WebSocketException e) when(e.WebSocketErrorCode == WebSocketError.NotAWebSocket && e.Message.Contains("404"))
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, "Invalid link"));

            Log.Error(e, "The URI {Uri} not a correct websocket address", uri);
        }
        catch (WebSocketException e) when(e.WebSocketErrorCode == WebSocketError.NotAWebSocket && e.Message.Contains("429"))
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, "Rate limit exceed. Wait a little time."));

            Log.Error(e, "The URI {Uri} rate limit exceed. Wait a little time", uri);
        }
        catch (WebSocketException e)
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, "Invalid link"));

            Log.Error(e, "The URI {Uri} occured error", uri);
        }
        catch (TaskCanceledException)
        {
            Log.Information("Connecting to web socket for order {OrderName} was cancelled", _order.Name);
        }
        catch (Exception e)
        {
            _orderErrorChannelWriter.TryWrite(new OrderError(_order.Id, e.Message));

            Log.Error(e, "Error connecting to web socket for order {OrderName}", _order.Name);
        }
        
        OnConnectionFailed?.Invoke(this, EventArgs.Empty);
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
}