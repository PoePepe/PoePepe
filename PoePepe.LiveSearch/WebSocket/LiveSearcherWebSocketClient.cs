using System.Net.WebSockets;
using System.Text.Json;
using PoePepe.LiveSearch.Api;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.Services;
using Serilog;

namespace PoePepe.LiveSearch.WebSocket;

/// <summary>
/// Represents a WebSocket client for live searching in the Path of Exile game.
/// </summary>
public class LiveSearcherWebSocketClient : IDisposable
{
    /// <summary>
    /// Represents a client-side WebSocket connection.
    /// </summary>
    private readonly ClientWebSocket _clientWebSocket;

    /// <summary>
    /// Represents the order object.
    /// </summary>
    private readonly Order _order;

    /// <summary>
    /// Holds the options for interacting with the Path of Exile API.
    /// </summary>
    private readonly PoeApiOptions _poeApiOptions;

    /// <summary>
    /// Represents the current state of the service.
    /// </summary>
    private readonly ServiceState _serviceState;

    /// <summary>
    /// Represents the current state of the connection.
    /// </summary>
    private bool _isConnected;

    /// <summary>
    /// Event raised when the connection is established.
    /// </summary>
    public EventHandler OnConnected;

    /// <summary>
    /// Event that is raised when a connection attempt fails.
    /// </summary>
    public EventHandler OnConnectionFailed;

    /// <summary>
    /// Occurs when the order processing fails.
    /// </summary>
    public EventHandler<OrderProcessingFailedEventArgs> OnProcessingFailed;

    /// <summary>
    /// Initializes a new instance of the LiveSearcherWebSocketClient class.
    /// </summary>
    /// <param name="order">The Order object.</param>
    /// <param name="poeApiOptions">The PoeApiOptions object.</param>
    /// <param name="serviceState">The ServiceState object.</param>
    public LiveSearcherWebSocketClient(Order order, PoeApiOptions poeApiOptions, ServiceState serviceState)
    {
        _poeApiOptions = poeApiOptions;
        _serviceState = serviceState;

        _order = order;
        _clientWebSocket = new ClientWebSocket();

        SetHeaders();
    }

    /// <summary>
    /// Disposes the client WebSocket if it is not null.
    /// </summary>
    public void Dispose()
    {
        _clientWebSocket?.Dispose();
    }

    /// <summary>
    /// Tries to establish a connection asynchronously.
    /// </summary>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating whether the connection was successfully established.</returns>
    public Task<bool> TryConnectAsync(CancellationToken token = default) => ConnectCoreAsync(token);

    /// <summary>
    /// Initiates an asynchronous connection attempt.
    /// </summary>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous connection attempt. The task result indicates whether the connection attempt was successful or not.</returns>
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

    /// <summary>
    /// Starts receiving data asynchronously from the web socket for the specified order.
    /// </summary>
    /// <param name="token">The cancellation token to stop receiving data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
            _serviceState.OrderErrorChannel.Writer.TryWrite(new OrderError(_order.Id, e.Message, OrderErrorType.Process));

            Log.Error(e, "Error receiving data from web socket for order {OrderName}", _order.Name);

            OnProcessingFailed?.Invoke(this, new OrderProcessingFailedEventArgs(_order.Id));
        }
    }

    /// <summary>
    /// Sets the headers for the client WebSocket connection.
    /// </summary>
    private void SetHeaders()
    {
        _clientWebSocket.Options.SetRequestHeader("Cookie", _serviceState.Session);
        _clientWebSocket.Options.SetRequestHeader("Origin", "https://www.pathofexile.com");
        _clientWebSocket.Options.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
    }

    /// <summary>
    /// Connects to the web socket asynchronously.
    /// </summary>
    /// <param name="token">Optional cancellation token (default is <see cref="CancellationToken.None" />).</param>
    /// <returns>
    /// Returns a task that represents the asynchronous connection operation. The task will return <c>true</c> if the connection was successful,
    /// otherwise <c>false</c>.
    /// </returns>
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
            _serviceState.OrderErrorChannel.Writer.TryWrite(new OrderError(_order.Id, "Invalid link"));

            Log.Error(e, "The URI {Uri} not a correct websocket address", uri);

            return true;
        }
        catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.NotAWebSocket &&
                                           e.Message.Contains("429"))
        {
            _serviceState.OrderErrorChannel.Writer.TryWrite(new OrderError(_order.Id, "Rate limit exceed. Wait a little time."));

            Log.Error(e, "The URI {Uri} rate limit exceed. Wait a little time", uri);
        }
        catch (WebSocketException e)
        {
            _serviceState.OrderErrorChannel.Writer.TryWrite(new OrderError(_order.Id, $"Connection error: {e.Message}"));

            Log.Error(e, "The URI {Uri} occured error {Error}", uri, e.Message);
        }
        catch (TaskCanceledException)
        {
            Log.Information("Connecting to web socket for order {OrderName} was cancelled", _order.Name);

            return true;
        }
        catch (Exception e)
        {
            _serviceState.OrderErrorChannel.Writer.TryWrite(new OrderError(_order.Id, e.Message));

            Log.Error(e, "Error connecting to web socket for order {OrderName}", _order.Name);
        }

        return false;
    }

    /// <summary>
    /// Processes the received message by deserializing the message stream into a LiveResponse object.
    /// If the message contains a list of item IDs, it creates an ItemLiveResponse for each ID and writes it to a channel.
    /// Finally, it disposes the message stream.
    /// </summary>
    /// <param name="messageStream">The stream that contains the message to be processed.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessReceivedMessageAsync(Stream messageStream, CancellationToken cancellationToken)
    {
        var message =
            await JsonSerializer.DeserializeAsync<LiveResponse>(messageStream, cancellationToken: cancellationToken);

        if (message?.ItemIds is not null)
        {
            foreach (var itemId in message.ItemIds)
            {
                var itemLiveResponse = new ItemLiveResponse(itemId, _order.Id, _order.Name);
                await _serviceState.LiveSearchChannel.Writer.WriteAsync(itemLiveResponse, cancellationToken);
            }

            Log.Debug("Received item of order {OrderName}", _order.Name);
        }

        await messageStream.DisposeAsync();
    }
}