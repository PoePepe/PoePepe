using Microsoft.Extensions.Options;
using Poe.LiveSearch.Api;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Persistence;
using Poe.LiveSearch.WebSocket;
using Serilog;

namespace Poe.LiveSearch.Services;

public class Service
{
    private readonly PoeTradeApiService _poeTradeApiService;

    private readonly ServiceState _serviceState;
    private readonly IOrderRepository _orderRepository;
    private readonly IItemHistoryRepository _itemHistoryRepository;
    private readonly PoeApiOptions _poeApiOptions;

    public Service(PoeTradeApiService poeTradeApiService, ServiceState serviceState, IOrderRepository orderRepository, IOptions<PoeApiOptions> options, IItemHistoryRepository itemHistoryRepository)
    {
        _poeTradeApiService = poeTradeApiService;
        _serviceState = serviceState;
        _orderRepository = orderRepository;
        _itemHistoryRepository = itemHistoryRepository;
        _poeApiOptions = options.Value;
    }

    public Order CreateOrder(Order order)
    {
        _orderRepository.Add(order);
        return order;
    }
    
    public Order UpdateOrder(Order order)
    {
        _orderRepository.Update(order);
        return order;
    }
    
    public void UpdateManyMod(IEnumerable<long> orderIds, OrderMod mod)
    {
        _orderRepository.UpdateManyMod(orderIds, mod);
    }

    public void UpdateAllMod(OrderMod mod)
    {
        _orderRepository.UpdateAllMod(mod);
    }

    public Order GetOrder(long id)
    {
        return _orderRepository.GetById(id);
    }
    
    public IEnumerable<Order> GetOrders()
    {
        return _orderRepository.GetAll();
    }

    public IEnumerable<Order> GetOrdersByLeague(string leagueName)
    {
        return _orderRepository.GetAll().Where(x => x.LeagueName == leagueName);
    }
    
    public void StartLiveSearchAsync(IEnumerable<Order> orders)
    {
        foreach (var order in orders.Where(x => x.Activity == OrderActivity.Enabled))
        {
            AddLiveSearchOrder(order);
        }
    }
    
    public async Task StartLiveSearchAsync(long orderId)
    {
        var order = _orderRepository.GetById(orderId);
        // var searchResponseResult = await _poeTradeApiService.SearchItemsAsync(order.QueryHash);
        // if (!searchResponseResult.IsSuccess)
        // {
        //     //exit
        // }
        //
        // var searchResponse = searchResponseResult.Content;
        //
        // var fetchResponse = await _poeTradeApiService.FetchItemsAsync(string.Join(',', searchResponse.Result.Take(10)), searchResponse.Id);
        // if (!fetchResponse.IsSuccess)
        // {
        //     //log warning
        //     //continue
        // }

        // var da = fetchResponse.Content.Results.FirstOrDefault();
        // await _serviceState.FoundItemsChannel.Writer.WriteAsync(da);

        AddLiveSearchOrder(order);
    }
    
    public void StopSearchingForOrders(string leagueName = null)
    {
        var orders = _orderRepository.GetAll().Where(x => x.LeagueName == leagueName);
        foreach (var order in orders)
        {
            StopSearchingForOrder(order.Id);
        }
    }
    
    public void ClearAllOrders()
    {
        var orders = _orderRepository.GetAll();
        foreach (var order in orders)
        {
            StopSearchingForOrder(order.Id);
        }

        _orderRepository.Clear();
        _itemHistoryRepository.Clear();
    }
    
    public bool DeleteOrder(long orderId)
    {
        StopSearchingForOrder(orderId);

        _itemHistoryRepository.ClearByOrder(orderId);

        return _orderRepository.Delete(orderId);
    }
    
    public void TryEnableLiveSearchOrder(long orderId, Action<object, EventArgs> onConnected, Action<object, EventArgs> onConnectionFailed)
    {
        var order = _orderRepository.GetById(orderId);

        if (order is null)
        {
            Log.Warning("The order {OrderId} not found", orderId);

            return;
        }

        AddLiveSearchOrder(order, onConnected, onConnectionFailed);
    }

    public void EnableLiveSearchOrder(long orderId)
    {
        var order = _orderRepository.GetById(orderId);

        if (order is null)
        {
            Log.Warning("The order {OrderId} not found", orderId);

            return;
        }

        if (order.Activity == OrderActivity.Enabled)
        {
            Log.Warning("The order {OrderName} is already enabled", order.Name);

            return;
        }

        order.Activity = OrderActivity.Enabled;
        _orderRepository.Update(order);

        AddLiveSearchOrder(order);
    }

    public void DisableLiveSearchOrder(long orderId)
    {
        var order = _orderRepository.GetById(orderId);
                
        if (order is null)
        {
            Log.Warning("The order {OrderId} not found", orderId);

            return;
        }

        if (order.Activity == OrderActivity.Disabled)
        {
            Log.Warning("The order {OrderName} is already disabled", order.Name);

            return;
        }

        order.Activity = OrderActivity.Disabled;
        _orderRepository.Update(order);

        StopSearchingForOrder(orderId);
    }
    
    private void AddLiveSearchOrder(Order order, Action<object, EventArgs> onConnected = null, Action<object, EventArgs> onConnectionFailed = null)
    {
        if (_serviceState.LiveSearches.ContainsKey(order.Id))
        {
            Log.Warning("The search by order {OrderName} already exists", order.Name);

            return;
        }

        var data = new SubscriptionData
        {
            Order = order,
            CancellationTokenSource = new CancellationTokenSource()
        };

        var token = data.CancellationTokenSource.Token;

        if (_serviceState.LiveSearches.TryAdd(order.Id, data))
        {
            var client = new LiveSearcherWebSocketClient(_poeApiOptions, _serviceState);

            if (onConnected is not null)
            {
                client.OnConnected += (sender, args) => onConnected(sender, args);
            }
            
            if (onConnectionFailed is not null)
            {
                client.OnConnectionFailed += (sender, args) => onConnectionFailed(sender, args);
            }

            Task.Run(async () =>
            {
                await client.ConnectAsync(order, token);

                await client.StartReceiveAsync(token);
            }, token);
            
            Log.Information("Сreated a search by order {OrderName}", order.Name);
        }
    }

    private void OnConnected(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void StopSearchingForOrder(long orderId)
    {
        if (_serviceState.LiveSearches.TryRemove(orderId, out var data))
        {
            data.CancellationTokenSource.Cancel();
            
            Log.Information("The search by order {OrderName} is stopped", data.Order.Name);
        }
    }
}