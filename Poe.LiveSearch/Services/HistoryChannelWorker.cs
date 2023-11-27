using System.Threading.Channels;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Persistence;
using Serilog;

namespace Poe.LiveSearch.Services;

public class HistoryChannelWorker
{
    private readonly ChannelReader<FetchResponseResult> _historyItemsChannel;
    private readonly IItemHistoryRepository _itemHistoryRepository;

    public HistoryChannelWorker(ServiceState serviceState, IItemHistoryRepository itemHistoryRepository)
    {
        _itemHistoryRepository = itemHistoryRepository;

        _historyItemsChannel = serviceState.HistoryItemsChannel.Reader;
    }

    public void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            while (await _historyItemsChannel.WaitToReadAsync(token))
            {
                while (_historyItemsChannel.TryRead(out var fetchResponse) && !token.IsCancellationRequested)
                {
                    await ProcessFoundItemsAsync(fetchResponse, token);
                }
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        Log.Information("Started receiving from channel of found items");
    }

    private ValueTask ProcessFoundItemsAsync(FetchResponseResult result, CancellationToken token)
    {
        try
        {
            Log.Debug("Recording history item of order {OrderName}", result.OrderName);

            if (token.IsCancellationRequested)
            {
                return ValueTask.CompletedTask;
            }

            var itemHistory = _itemHistoryRepository.GetByItemId(result.Id);

            if (itemHistory is null)
            {
                _itemHistoryRepository.Add(new ItemHistory
                {
                    OrderId = result.OrderId,
                    ItemId = result.Id,
                    FoundDate = DateTimeOffset.UtcNow,
                    ItemData = result
                });

                return ValueTask.CompletedTask;
            }

            itemHistory.FoundDate = DateTimeOffset.UtcNow;
            itemHistory.ItemData = result;
            _itemHistoryRepository.Update(itemHistory);

            Log.Debug("History item of order {OrderName} recorded", result.OrderName);

            return ValueTask.CompletedTask;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error recording history item of order {OrderName}", result.OrderName);

            return ValueTask.CompletedTask;
        }
    }
}