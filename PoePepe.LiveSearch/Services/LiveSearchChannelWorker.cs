using PoePepe.LiveSearch.Api.Trade;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.Persistence;
using PoePepe.LiveSearch.WebSocket;
using Serilog;

namespace PoePepe.LiveSearch.Services;

/// <summary>
/// Represents a worker class for live search channel.
/// </summary>
public class LiveSearchChannelWorker : IDisposable, IAsyncDisposable
{
    private readonly IOrderRepository _orderRepository;
    private readonly PoeTradeApiService _poeTradeApiService;
    private readonly ServiceState _serviceState;
    private Timer _timer;

    public LiveSearchChannelWorker(PoeTradeApiService poeTradeApiService, ServiceState state, IOrderRepository orderRepository)
    {
        _poeTradeApiService = poeTradeApiService;
        _serviceState = state;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Disposes the timer asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_timer != null) await _timer.DisposeAsync();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose();
    }

    /// <summary>
    /// Stops the receiving of searched items from the channel.
    /// </summary>
    public void Stop()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);

        Dispose();
        
        Log.Information("Stopped receiving from channel of searched items");

    }

    /// <summary>
    /// Starts receiving items from the channel of searched items.
    /// </summary>
    /// <param name="token">The cancellation token to stop receiving items.</param>
    public void Start(CancellationToken token)
    {
        Log.Information("Started receiving from channel of searched items");

        _timer = new Timer(_ => Task.Run(async () =>
        {
            try
            {
                await ReceiveSearchedItemsAsync(token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Log.Error(e, "Error processing from channel of searched items");
            }
        }, token), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
    }

    /// <summary>
    /// Receives searched items asynchronously.
    /// </summary>
    /// <param name="token">Cancellation token to stop the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ReceiveSearchedItemsAsync(CancellationToken token)
    {
        try
        {
            Log.Verbose("Receiving channel of searched items");

            var batch = new List<ItemLiveResponse>(10);
            while (batch.Count < 10 && _serviceState.LiveSearchChannel.Reader.TryRead(out var liveResponse))
            {
                if (_orderRepository.GetById(liveResponse.OrderId).Activity == OrderActivity.Enabled)
                {
                    batch.Add(liveResponse);
                }
            }

            if (!batch.Any())
            {
                Log.Verbose("No items for processing from channel of searched items");

                return;
            }
            
            await ProcessItemsAsync(batch, token);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error processing from channel of searched items");
        }
    }

    /// <summary>
    /// Process a batch of items asynchronously.
    /// </summary>
    /// <param name="batch">The list of items to process.</param>
    /// <param name="token">A cancellation token to stop the process if needed.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task ProcessItemsAsync(List<ItemLiveResponse> batch, CancellationToken token)
    {
        try
        {
            if (!batch.Any())
            {
                Log.Verbose("No items for processing from channel of searched items");

                return;
            }
        
            Log.Debug("Found {Count} items for processing for fetch request", batch.Count);
        
            var itemIds = string.Join(',', batch.Select(x => x.ItemId).Distinct());
        
            var items = await _poeTradeApiService.FetchItemsAsync(itemIds);
            if (items.IsSuccess)
            {
                var tasks = new List<ValueTask>();
                foreach (var result in items.Content.Results)
                {
                    var orders = batch.Where(x => x.ItemId == result.Id);
                    result.Orders = orders;
                
                    tasks.Add(_serviceState.FoundItemsChannel.Writer.WriteAsync(result, token));
                }
            
                await WhenAll(tasks);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error processing from channel of searched items");
        }
    }

    /// <summary>
    /// Waits for all the tasks in a list of ValueTask objects to complete.
    /// </summary>
    /// <param name="tasks">The list of ValueTask objects to wait for.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public static async ValueTask WhenAll(List<ValueTask> tasks)
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