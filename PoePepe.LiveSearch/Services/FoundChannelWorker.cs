using PoePepe.LiveSearch.Api.Trade.Models;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.Persistence;
using PoePepe.LiveSearch.WebSocket;
using Serilog;

namespace PoePepe.LiveSearch.Services;

/// <summary>
/// Represents a worker class that processes found channel items.
/// </summary>
public class FoundChannelWorker
{
    private readonly IOrderRepository _orderRepository;
    private readonly ServiceState _serviceState;

    public FoundChannelWorker(ServiceState serviceState, IOrderRepository orderRepository)
    {
        _serviceState = serviceState;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Starts the receiving process from the channel of found items.
    /// </summary>
    /// <param name="token">The cancellation token to stop the receiving process.</param>
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

    /// <summary>
    /// Processes the found items asynchronously.
    /// </summary>
    /// <param name="result">The fetch response result.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    private ValueTask ProcessFoundItemsAsync(FetchResponseResult result, CancellationToken token)
    {
        var tasks = new List<ValueTask>();
        foreach (var orderInfo in result.Orders)
        {
            tasks.Add(ProcessFoundItemsAsync(result, orderInfo, token));
        }

        return WhenAll(tasks);
    }

    /// <summary>
    /// Process the found items asynchronously.
    /// </summary>
    /// <param name="result">The fetch response result.</param>
    /// <param name="orderInfo">The item live response.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Executes a collection of tasks concurrently and returns a task that represents the completion of all the tasks.
    /// </summary>
    /// <param name="tasks">The tasks to execute concurrently.</param>
    /// <returns>A task representing the completion of all the tasks.</returns>
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

    /// <summary>
    /// Executes multiple <see cref="ValueTask"/> instances concurrently.
    /// </summary>
    /// <param name="tasks">The list of <see cref="ValueTask"/> instances to execute.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous execution of all the tasks.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tasks"/> is null.</exception>
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