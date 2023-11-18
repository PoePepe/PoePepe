using System.Collections.Concurrent;
using System.Threading.Channels;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.WebSocket;

namespace Poe.LiveSearch.Services;

public class ServiceState
{
    public ConcurrentDictionary<long, SubscriptionData> LiveSearches { get; } = new();
    public Channel<ItemLiveResponse> LiveSearchChannel { get; } = Channel.CreateUnbounded<ItemLiveResponse>();
    public Channel<FetchResponseResult> FoundItemsChannel { get; } = Channel.CreateUnbounded<FetchResponseResult>();
    public Channel<FetchResponseResult> NotificationItemsChannel { get; } = Channel.CreateUnbounded<FetchResponseResult>();

    public Channel<WhisperRequestData> WhisperItemsChannel { get; } = Channel.CreateUnbounded<WhisperRequestData>();
    public Channel<OrderError> OrderErrorChannel { get; } = Channel.CreateUnbounded<OrderError>();
}