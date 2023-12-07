using PoePepe.LiveSearch.Api.Trade.Models;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.Persistence;
using Serilog;

namespace PoePepe.LiveSearch.Services;

/// <summary>
/// Represents a worker class for processing history items.
/// </summary>
public class HistoryChannelWorker
{
    private readonly IItemHistoryRepository _itemHistoryRepository;
    private readonly ServiceState _serviceState;

    public HistoryChannelWorker(ServiceState serviceState, IItemHistoryRepository itemHistoryRepository)
    {
        _serviceState = serviceState;
        _itemHistoryRepository = itemHistoryRepository;
    }

    /// <summary>
    /// Starts processing the history order queue by reading items from the history items channel.
    /// </summary>
    /// <param name="token">The cancellation token used to stop the processing.</param>
    public void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            try
            {
                while (await _serviceState.HistoryItemsChannel.Reader.WaitToReadAsync(token))
                {
                    while (_serviceState.HistoryItemsChannel.Reader.TryRead(out var fetchResponse) && !token.IsCancellationRequested)
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
                Log.Error(e, "Error processing history order queue");
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        Log.Information("Started receiving from channel of found items");
    }

    /// <summary>
    /// Processes the found items asynchronously.
    /// </summary>
    /// <param name="result">The FetchResponseResult containing the information about the found item.</param>
    /// <param name="token">The cancellation token that can be used to cancel the operation.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
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