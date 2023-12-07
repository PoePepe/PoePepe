using PoePepe.LiveSearch.Api.Trade;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.Persistence;
using Serilog;

namespace PoePepe.LiveSearch.Services;

public class Service
{
    private readonly IItemHistoryRepository _itemHistoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly PoeTradeApiService _poeTradeApiService;

    private readonly ServiceState _serviceState;

    public Service(PoeTradeApiService poeTradeApiService, ServiceState serviceState, IOrderRepository orderRepository, IItemHistoryRepository itemHistoryRepository)
    {
        _poeTradeApiService = poeTradeApiService;
        _serviceState = serviceState;
        _orderRepository = orderRepository;
        _itemHistoryRepository = itemHistoryRepository;
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

    public Order GetOrder(long id) => _orderRepository.GetById(id);

    public IEnumerable<Order> GetOrders() => _orderRepository.GetAll();

    public async Task StartLiveSearchAsync(IEnumerable<Order> orders)
    {
        foreach (var order in orders.Where(x => x.Activity == OrderActivity.Enabled))
        {
            await AddLiveSearchOrderAsync(order);
        }
    }

    public async Task StartLiveSearchAsync(long orderId)
    {
        var order = _orderRepository.GetById(orderId);

        await AddLiveSearchOrderAsync(order);
    }

    public void StopSearchingForOrders()
    {
        var orders = _orderRepository.GetAll();
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

    public async Task EnableLiveSearchOrder(long orderId)
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

        await AddLiveSearchOrderAsync(order);
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

    private async Task AddLiveSearchOrderAsync(Order order)
    {
        await _serviceState.OrderStartSearchChannel.Writer.WriteAsync(order);
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