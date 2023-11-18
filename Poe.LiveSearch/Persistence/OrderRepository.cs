﻿#nullable enable
using System.Collections.Concurrent;
using LiteDB;
using Poe.LiveSearch.Models;

namespace Poe.LiveSearch.Persistence;

public class OrderRepository : IOrderRepository
{
    private readonly ILiteCollection<Order> _collection;
    private readonly ConcurrentDictionary<long, Order> _cache;
    
    public OrderRepository(ILiteDbContext liteDbContext)
    {
        _collection = liteDbContext.Database.GetCollection<Order>("Order");

        var da = _collection.Find(Query.All()).ToArray();
        
        _cache = new ConcurrentDictionary<long, Order>(da.ToDictionary(x => x.Id, y => y));
    }

    public IEnumerable<Order> GetAll()
    {
        return _cache.Values;
    }

    public Order? GetById(long id)
    {
        return !_cache.TryGetValue(id, out var order) ? null : order;
    }

    public void Clear()
    {
        _cache.Clear();

        _collection.DeleteAll();
    }

    public IQueryable<Order> Get()
    {
        return _cache.Values.AsQueryable();
    }

    public Order Add(Order entity)
    {
        if (_cache.TryAdd(entity.Id, entity))
        {
            _collection.Insert(entity);
        }

        return entity;
    }

    public Order Update(Order entity)
    {
        if (!_cache.TryGetValue(entity.Id, out var oldEntity))
        {
            return entity;
        }

        oldEntity.QueryHash = entity.QueryHash;
        oldEntity.Name = entity.Name;
        oldEntity.OrderPrice = entity.OrderPrice;
        oldEntity.Activity = entity.Activity;
        oldEntity.Mod = entity.Mod;
            
        _collection.Update(entity.Id, entity);

        return entity;
    }

    public bool Delete(long id)
    {
        if (_cache.TryRemove(id, out _))
        {
            return _collection.Delete(id);
        }

        return false;
    }
}