using LiteDB;
using Poe.LiveSearch.Models;

namespace Poe.LiveSearch.Persistence;

public class ItemHistoryRepository : IItemHistoryRepository
{
    private readonly ILiteCollection<ItemHistory> _collection;

    public ItemHistoryRepository(ILiteDbContext liteDbContext)
    {
        _collection = liteDbContext.Database.GetCollection<ItemHistory>("ItemHistory");
    }

    public IEnumerable<ItemHistory> GetByOrderId(long orderId, int top, int skip)
    {
        return _collection.Query().OrderByDescending(x => x.FoundDate).Where(x => x.OrderId == orderId).Limit(top).Skip(skip).ToEnumerable();
    }

    public ItemHistory GetByItemId(string itemId)
    {
        return _collection.Query().Where(x => x.ItemId == itemId).Select(x => new ItemHistory
        {
            OrderId = x.OrderId,
            ItemId = x.ItemId,
            FoundDate = x.FoundDate
        }).FirstOrDefault();
    }
    
    public ItemHistory GetFullByItemId(string itemId)
    {
        return _collection.Query().Where(x => x.ItemId == itemId).FirstOrDefault();
    }

    public ItemHistory Add(ItemHistory entity)
    {
        _collection.Insert(entity);

        return entity;
    }

    public ItemHistory Update(ItemHistory entity)
    {
        _collection.Update(entity.ItemId, entity);

        return entity;
    }

    public void Clear()
    {
        _collection.DeleteAll();
    }

    public void ClearByOrder(long orderId)
    {
        _collection.DeleteMany(x => x.OrderId == orderId);
    }
}