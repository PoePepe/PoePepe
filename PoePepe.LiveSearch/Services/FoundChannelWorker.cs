using PoePepe.LiveSearch.Api.Trade.Models;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.Persistence;
using PoePepe.LiveSearch.WebSocket;
using Serilog;

namespace PoePepe.LiveSearch.Services;

public class FoundChannelWorker
{
    private readonly IOrderRepository _orderRepository;
    private readonly ServiceState _serviceState;

    public FoundChannelWorker(ServiceState serviceState, IOrderRepository orderRepository)
    {
        _serviceState = serviceState;
        _orderRepository = orderRepository;
    }

    public void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            try
            {
                while (await _serviceState.FoundItemsChannel.Reader.WaitToReadAsync(token))
                {
                    while (_serviceState.FoundItemsChannel.Reader.TryRead(out var fetchResponse) &&
                           !token.IsCancellationRequested)
                    {
                        await ProcessFoundItemsAsync(fetchResponse, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Log.Error(e, "Error processing found orders");
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        
        Log.Information("Started receiving from channel of found items");
    }

    private ValueTask ProcessFoundItemsAsync(FetchResponseResult result, CancellationToken token)
    {
        var tasks = new List<ValueTask>();
        foreach (var orderInfo in result.Orders)
        {
            tasks.Add(ProcessFoundItemsAsync(result, orderInfo, token));
        }

        return WhenAll(tasks);
    }

    private ValueTask ProcessFoundItemsAsync(FetchResponseResult result,  ItemLiveResponse orderInfo, CancellationToken token)
    {
        try
        {
            Log.Debug("Processing found item of order {OrderName}", orderInfo.OrderName);

            if (token.IsCancellationRequested)
            {
                return ValueTask.CompletedTask;
            }

            var order = _orderRepository.GetById(orderInfo.OrderId);
        
            if (order is null)
            {
                Log.Warning("Order {OrderId} {OrderName} not found", orderInfo.OrderId, orderInfo.OrderName);

                return ValueTask.CompletedTask;
            }
            result.OrderName = orderInfo.OrderName;
            result.OrderId = orderInfo.OrderId;
            var historyTask = _serviceState.HistoryItemsChannel.Writer.WriteAsync(result, token);

            if (order.Activity == OrderActivity.Disabled)
            {
                Log.Warning("Order {OrderName} is disabled", orderInfo.OrderName);

                return ValueTask.CompletedTask;
            }
        
            if (order.Mod == OrderMod.Whisper)
            {
                Log.Debug("Order {OrderName} sent to whisper channel", orderInfo.OrderName);

                var whisperTask = _serviceState.WhisperItemsChannel.Writer.WriteAsync(
                    new WhisperRequestData(new WhisperRequest(result.Listing.WhisperToken), order.Name), token);

                return WhenAll(whisperTask, historyTask);
            }

            Log.Debug("Order {OrderName} sent to notification channel", orderInfo.OrderName);

            var notificationTask = _serviceState.NotificationItemsChannel.Writer.WriteAsync(result, token);

            return WhenAll(notificationTask, historyTask);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error processing found item of order {OrderName}", orderInfo.OrderName);
            
            return ValueTask.CompletedTask;
        }
    }

    private static async ValueTask WhenAll(params ValueTask[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        if (tasks.Length == 0)
        {
            return;
        }

        for (var i = 0; i < tasks.Length; i++)
        {
            await tasks[i].ConfigureAwait(false);
        }
    }

    private static async ValueTask WhenAll(IReadOnlyList<ValueTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        if (tasks.Count == 0)
        {
            return;
        }

        for (var i = 0; i < tasks.Count; i++)
        {
            await tasks[i].ConfigureAwait(false);
        }
    }
}