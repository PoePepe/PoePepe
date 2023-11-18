using System.Threading.Channels;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Persistence;
using Poe.LiveSearch.WebSocket;
using Serilog;

namespace Poe.LiveSearch.Services;
// todo come up mechanism of workers
public class FoundChannelWorker
{
    private readonly ServiceState _serviceState;
    private readonly ChannelReader<FetchResponseResult> _foundItemsChannel;
    private readonly ChannelWriter<FetchResponseResult> _notificationItemsChannel;
    private readonly ChannelWriter<WhisperRequestData> _whisperItemsChannel;
    private readonly IOrderRepository _orderRepository;

    public FoundChannelWorker(ServiceState serviceState, IOrderRepository orderRepository)
    {
        _serviceState = serviceState;
        _orderRepository = orderRepository;

        _foundItemsChannel = serviceState.FoundItemsChannel.Reader;
        _notificationItemsChannel = serviceState.NotificationItemsChannel.Writer;
        _whisperItemsChannel = serviceState.WhisperItemsChannel.Writer;
    }

    public void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            while (await _foundItemsChannel.WaitToReadAsync(token))
            {
                while (_foundItemsChannel.TryRead(out var fetchResponse) && !token.IsCancellationRequested)
                {
                    await ProcessFoundItemsAsync(fetchResponse, token);
                }
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

            if (order.Activity == OrderActivity.Disabled)
            {
                Log.Warning("Order {OrderName} is disabled", orderInfo.OrderName);

                return ValueTask.CompletedTask;
            }
        
            if (order.Mod == OrderMod.Whisper)
            {
                Log.Debug("Order {OrderName} sent to whisper channel", orderInfo.OrderName);

                return _whisperItemsChannel.WriteAsync(new WhisperRequestData(new WhisperRequest(result.Listing.WhisperToken), order.Name), token);
            }

            Log.Debug("Order {OrderName} sent to notification channel", orderInfo.OrderName);

            result.OrderName = orderInfo.OrderName;
            result.OrderId = orderInfo.OrderId;
            return _notificationItemsChannel.WriteAsync(result, token);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error processing found item of order {OrderName}", orderInfo.OrderName);
            
            return ValueTask.CompletedTask;
        }
    }
    
    private static async ValueTask WhenAll(List<ValueTask> tasks)
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