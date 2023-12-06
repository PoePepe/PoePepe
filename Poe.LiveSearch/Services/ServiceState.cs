using System.Collections.Concurrent;
using System.Threading.Channels;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.WebSocket;

namespace Poe.LiveSearch.Services;

public class ServiceState
{
    public string LeagueName { get; set; }
    public string Session { get; set; }
    public ConcurrentDictionary<long, SubscriptionData> LiveSearches { get; } = new();
    public Channel<ItemLiveResponse> LiveSearchChannel { get; private set; } = Channel.CreateUnbounded<ItemLiveResponse>();
    public Channel<FetchResponseResult> FoundItemsChannel { get; private set; } = Channel.CreateUnbounded<FetchResponseResult>();
    public Channel<FetchResponseResult> NotificationItemsChannel { get; private set; } = Channel.CreateUnbounded<FetchResponseResult>();
    public Channel<FetchResponseResult> HistoryItemsChannel { get; private set; } = Channel.CreateUnbounded<FetchResponseResult>();

    public Channel<WhisperRequestData> WhisperItemsChannel { get; private set; } = Channel.CreateUnbounded<WhisperRequestData>();
    public Channel<OrderError> OrderErrorChannel { get; private set; } = Channel.CreateUnbounded<OrderError>();

    public Channel<Order> OrderStartSearchChannel { get; private set; } = Channel.CreateUnbounded<Order>();

    public void ClearOrderProcessingChannels()
    {
        LiveSearchChannel = Channel.CreateUnbounded<ItemLiveResponse>();
        FoundItemsChannel = Channel.CreateUnbounded<FetchResponseResult>();
        NotificationItemsChannel = Channel.CreateUnbounded<FetchResponseResult>();
        WhisperItemsChannel = Channel.CreateUnbounded<WhisperRequestData>();
        OrderStartSearchChannel = Channel.CreateUnbounded<Order>();
    }
}