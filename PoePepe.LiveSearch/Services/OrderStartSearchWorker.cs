using Microsoft.Extensions.Options;
using PoePepe.LiveSearch.Api;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.WebSocket;
using Serilog;

namespace PoePepe.LiveSearch.Services;

public class OrderStartSearchWorker
{
    private readonly PoeApiOptions _poeApiOptions;
    private readonly ServiceState _serviceState;

    private SubscriptionData _currentWorkingOrderState;

    private Timer _timer;

    public OrderStartSearchWorker(ServiceState serviceState, IOptions<PoeApiOptions> poeApiOptions)
    {
        _serviceState = serviceState;
        _poeApiOptions = poeApiOptions.Value;
    }

    public void Start(CancellationToken token = default)
    {
        _timer = new Timer(_ => Task.Run(async () => await TryStartWork(token), token), null, Timeout.Infinite,
            Timeout.Infinite);

        Task.Factory.StartNew(async () =>
        {
            try
            {
                var isPreviousAttemptSuccess = true;
                while (isPreviousAttemptSuccess &&
                       await _serviceState.OrderStartSearchChannel.Reader.WaitToReadAsync(token))
                {
                    while (isPreviousAttemptSuccess &&
                           _serviceState.OrderStartSearchChannel.Reader.TryRead(out var order) &&
                           !token.IsCancellationRequested)
                    {
                        isPreviousAttemptSuccess = await AddLiveSearchOrderAsync(order);
                        if (!isPreviousAttemptSuccess)
                        {
                            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Log.Error(e, "Error starting order queue");
            }
            
        }, token);
    }

    private async Task TryStartWork(CancellationToken token = default)
    {
        Log.Information("Trying to restart orders connecting...");
        var isConnected = await _currentWorkingOrderState.Client.TryConnectAsync(token);
        if (isConnected)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    private async Task<bool> AddLiveSearchOrderAsync(Order order)
    {
        if (_serviceState.LiveSearches.ContainsKey(order.Id))
        {
            Log.Warning("The search by order {OrderName} already exists", order.Name);

            return true;
        }

        var data = new SubscriptionData
        {
            Order = order
        };

        if (!_serviceState.LiveSearches.TryAdd(order.Id, data))
        {
            Log.Warning("The search by order {OrderName} already exists", order.Name);

            return true;
        }

        data.CancellationTokenSource = new CancellationTokenSource();
        var token = data.CancellationTokenSource.Token;

        var client = new LiveSearcherWebSocketClient(order, _poeApiOptions, _serviceState);

        // client.OnConnected += OnConnected;
        // client.OnConnectionFailed += OnConnectionFailed;
        client.OnProcessingFailed += OnProcessingFailed;

        data.Client = client;

        _currentWorkingOrderState = data;

        var isConnected = await client.ConnectAsync(token);

        if (!isConnected)
        {
            Log.Warning("Connection failed with order {OrderName}", _currentWorkingOrderState.Order.Name);

            return false;
        }

        await Task.Factory.StartNew(async () => { await client.StartReceiveAsync(token); },
            token, TaskCreationOptions.LongRunning,
            TaskScheduler.Current);

        Log.Information("Сreated a search by order {OrderName}", order.Name);

        return true;
    }

    private void OnProcessingFailed(object sender, OrderProcessingFailedEventArgs e)
    {
        if (!_serviceState.LiveSearches.TryRemove(e.OrderId, out var data))
        {
            Log.Warning("The search order {OrderId} doesn't exists", e.OrderId);

            return;
        }

        data.Client.Dispose();
        data.CancellationTokenSource.Dispose();

        _serviceState.OrderStartSearchChannel.Writer.TryWrite(data.Order);

        Log.Debug("Failed search order {OrderName} sent for new connect", data.Order.Name);
    }
}