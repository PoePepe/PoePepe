using PoePepe.LiveSearch.Api.Trade;
using PoePepe.LiveSearch.Models;
using PoePepe.LiveSearch.Persistence;
using Serilog;

namespace PoePepe.LiveSearch.Services;

/// <summary>
/// Represents a service for managing orders and live searching.
/// </summary>
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

    /// <summary>
    /// Creates a new order in the system. </summary>
    /// <param name="order">The order object to be created.</param>
    /// <returns>The created order object.</returns>
    public Order CreateOrder(Order order)
    {
        _orderRepository.Add(order);
        return order;
    }

    /// <summary>
    /// Updates an order.
    /// </summary>
    /// <param name="order">The order to update.</param>
    /// <returns>The updated order.</returns>
    public Order UpdateOrder(Order order)
    {
        _orderRepository.Update(order);
        return order;
    }

    /// <summary>
    /// Updates multiple orders with the given mod.
    /// </summary>
    /// <param name="orderIds">The collection of order IDs.</param>
    /// <param name="mod">The order modification to apply.</param>
    public void UpdateManyMod(IEnumerable<long> orderIds, OrderMod mod)
    {
        _orderRepository.UpdateManyMod(orderIds, mod);
    }

    /// <summary>
    /// Updates all order modifications with the given mod.
    /// </summary>
    /// <param name="mod">The mod to update all order modifications with.</param>
    public void UpdateAllMod(OrderMod mod)
    {
        _orderRepository.UpdateAllMod(mod);
    }

    /// <summary>
    /// Retrieves an order by its ID.
    /// </summary>
    /// <param name="id">The ID of the order to retrieve.</param>
    /// <returns>The order with the specified ID.</returns>
    public Order GetOrder(long id) => _orderRepository.GetById(id);

    /// <summary>
    /// Retrieves all orders from the order repository.
    /// </summary>
    /// <returns>
    /// An enumerable collection of <see cref="Order"/> objects.
    /// </returns>
    public IEnumerable<Order> GetOrders() => _orderRepository.GetAll();

    /// <summary>
    /// Asynchronously starts a live search for the enabled orders in the given collection.
    /// </summary>
    /// <param name="orders">The collection of orders.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartLiveSearchAsync(IEnumerable<Order> orders)
    {
        foreach (var order in orders.Where(x => x.Activity == OrderActivity.Enabled))
        {
            await AddLiveSearchOrderAsync(order);
        }
    }

    /// <summary>
    /// Starts the live search asynchronously for the specified order.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartLiveSearchAsync(long orderId)
    {
        var order = _orderRepository.GetById(orderId);

        await AddLiveSearchOrderAsync(order);
    }

    /// <summary>
    /// Stops searching for all orders.
    /// </summary>
    public void StopSearchingForOrders()
    {
        var orders = _orderRepository.GetAll();
        foreach (var order in orders)
        {
            StopSearchingForOrder(order.Id);
        }
    }

    /// <summary>
    /// This method clears all orders in the system.
    /// It stops searching for each order and
    /// clears both the order repository and the item history repository.
    /// </summary>
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

    /// <summary>
    /// Deletes an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to be deleted.</param>
    /// <returns>Returns true if the order was successfully deleted; otherwise, false.</returns>
    public bool DeleteOrder(long orderId)
    {
        StopSearchingForOrder(orderId);

        _itemHistoryRepository.ClearByOrder(orderId);

        return _orderRepository.Delete(orderId);
    }

    /// <summary>
    /// Enables live search for the specified order.
    /// </summary>
    /// <param name="orderId">The ID of the order to enable live search for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Disables live search for the specified order.
    /// </summary>
    /// <param name="orderId">The ID of the order to disable live search for.</param>
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

    /// <summary>
    /// Adds an order to the live search channel asynchronously.
    /// </summary>
    /// <param name="order">The order to be added to the live search channel.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task AddLiveSearchOrderAsync(Order order)
    {
        await _serviceState.OrderStartSearchChannel.Writer.WriteAsync(order);
    }

    /// <summary>
    /// Stops searching for an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to stop searching for.</param>
    private void StopSearchingForOrder(long orderId)
    {
        if (_serviceState.LiveSearches.TryRemove(orderId, out var data))
        {
            data.CancellationTokenSource.Cancel();
            
            Log.Information("The search by order {OrderName} is stopped", data.Order.Name);
        }
    }
}