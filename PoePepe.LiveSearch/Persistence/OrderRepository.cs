using System.Collections.Concurrent;
using LiteDB;
using PoePepe.LiveSearch.Models;

namespace PoePepe.LiveSearch.Persistence;

/// <summary>
/// Represents a repository for managing orders.
/// </summary>
public class OrderRepository : IOrderRepository
{
    /// <summary>
    /// The dictionary used to cache orders.
    /// </summary>
    private readonly ConcurrentDictionary<long, Order> _cache;

    /// <summary>
    /// Private readonly ILiteCollection<Order> variable description.
    /// </summary>
    private readonly ILiteCollection<Order> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="liteDbContext">The LiteDB database context.</param>
    public OrderRepository(ILiteDbContext liteDbContext)
    {
        _collection = liteDbContext.Database.GetCollection<Order>("Order");

        var data = _collection.Find(Query.All()).ToArray();

        _cache = new ConcurrentDictionary<long, Order>(data.ToDictionary(x => x.Id, y => y));
    }

    /// <summary>
    /// Retrieves all the orders from the cache.
    /// </summary>
    /// <returns>
    /// An enumerable collection of Order objects.
    /// </returns>
    public IEnumerable<Order> GetAll() => _cache.Values;

    /// <summary>
    /// Retrieves an Order by its ID.
    /// </summary>
    /// <param name="id">The ID of the Order to retrieve.</param>
    /// <returns>
    /// The Order with the specified ID if found, otherwise null.
    /// </returns>
    public Order GetById(long id) => !_cache.TryGetValue(id, out var order) ? null : order;

    /// <summary>
    /// Clears the cache and deletes all items from the collection.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();

        _collection.DeleteAll();
    }

    /// <summary>
    /// Adds an order to the collection.
    /// </summary>
    /// <param name="entity">The order to add.</param>
    /// <returns>The added order.</returns>
    public Order Add(Order entity)
    {
        if (_cache.TryAdd(entity.Id, entity))
        {
            _collection.Insert(entity);
        }

        return entity;
    }

    /// <summary>
    /// Updates multiple orders with the specified modification.
    /// </summary>
    /// <param name="orderIds">An enumerable of order IDs.</param>
    /// <param name="mod">The modified order object.</param>
    public void UpdateManyMod(IEnumerable<long> orderIds, OrderMod mod)
    {
        _collection.UpdateMany(x => new Order
        {
            Mod = mod
        }, p => orderIds.Contains(p.Id) && p.Mod != mod);
    }

    /// <summary>
    /// Updates all orders with a new order modification.
    /// </summary>
    /// <param name="mod">The new order modification to be applied to the orders.</param>
    public void UpdateAllMod(OrderMod mod)
    {
        _collection.UpdateMany(x => new Order
        {
            Mod = mod
        }, p => p.Mod != mod);
    }

    /// <summary>
    /// Updates an order entity in the cache and the collection.
    /// </summary>
    /// <param name="entity">The updated order entity.</param>
    /// <returns>The updated order entity.</returns>
    public Order Update(Order entity)
    {
        if (!_cache.TryGetValue(entity.Id, out var oldEntity))
        {
            return entity;
        }

        oldEntity.QueryHash = entity.QueryHash;
        oldEntity.Name = entity.Name;
        oldEntity.Activity = entity.Activity;
        oldEntity.Mod = entity.Mod;
            
        _collection.Update(entity.Id, entity);

        return entity;
    }

    /// <summary>
    /// Deletes an item with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the item to delete.</param>
    /// <returns>true if the item was successfully deleted; otherwise, false.</returns>
    public bool Delete(long id)
    {
        if (_cache.TryRemove(id, out _))
        {
            return _collection.Delete(id);
        }

        return false;
    }
}