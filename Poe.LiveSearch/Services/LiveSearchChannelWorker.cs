using System.Threading.Channels;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Persistence;
using Poe.LiveSearch.WebSocket;
using Serilog;

namespace Poe.LiveSearch.Services;

public class LiveSearchChannelWorker : IDisposable, IAsyncDisposable
{
    private readonly IOrderRepository _orderRepository;
    private readonly PoeTradeApiService _poeTradeApiService;
    private readonly ChannelReader<ItemLiveResponse> _liveSearchChannelReader;
    private readonly ChannelWriter<FetchResponseResult> _foundItemsChannelWriter;
    private Timer _timer;

    public LiveSearchChannelWorker(PoeTradeApiService poeTradeApiService, ServiceState state, IOrderRepository orderRepository)
    {
        _poeTradeApiService = poeTradeApiService;
        _orderRepository = orderRepository;
        _liveSearchChannelReader = state.LiveSearchChannel.Reader;
        _foundItemsChannelWriter = state.FoundItemsChannel.Writer;
    }
    
    public void Stop()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);

        Dispose();
        
        Log.Information("Stopped receiving from channel of searched items");

    }
    
    public void Start(CancellationToken token)
    {
        Log.Information("Started receiving from channel of searched items");

        _timer = new Timer(_ => Task.Run(async () => await ReceiveSearchedItemsAsync(token), token), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
    }
    
    private async Task ReceiveSearchedItemsAsync(CancellationToken token)
    {
        try
        {
            Log.Verbose("Receiving channel of searched items");

            var batch = new List<ItemLiveResponse>(10);
            while (batch.Count < 10 && _liveSearchChannelReader.TryRead(out var liveResponse))
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
                
                    tasks.Add(_foundItemsChannelWriter.WriteAsync(result, token));
                }
            
                await WhenAll(tasks);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error processing from channel of searched items");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_timer != null) await _timer.DisposeAsync();
    }
    
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