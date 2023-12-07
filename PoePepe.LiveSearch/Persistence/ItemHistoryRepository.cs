using LiteDB;
using PoePepe.LiveSearch.Models;

namespace PoePepe.LiveSearch.Persistence;

/// <summary>
/// Represents a repository for managing item history records.
/// </summary>
public class ItemHistoryRepository : IItemHistoryRepository
{
    /// <summary>
    /// Represents a LiteDB collection of ItemHistory items.
    /// </summary>
    private readonly ILiteCollection<ItemHistory> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemHistoryRepository"/> class.
    /// </summary>
    /// <param name="liteDbContext">The instance of <see cref="ILiteDbContext"/> used to access the database.</param>
    public ItemHistoryRepository(ILiteDbContext liteDbContext)
    {
        _collection = liteDbContext.Database.GetCollection<ItemHistory>("ItemHistory");
    }

    /// <summary>
    /// Retrieves a list of <see cref="ItemHistory"/> objects based on the provided order ID, with optional pagination.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="top">The maximum number of records to retrieve.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <returns>
    /// A collection of <see cref="ItemHistory"/> objects sorted in descending order based on their found date,
    /// matching the provided order ID and respecting the specified pagination.
    /// </returns>
    public IEnumerable<ItemHistory> GetByOrderId(long orderId, int top, int skip)
    {
        return _collection.Query().OrderByDescending(x => x.FoundDate).Where(x => x.OrderId == orderId).Limit(top).Skip(skip).ToEnumerable();
    }

    /// <summary>
    /// Retrieves the item history based on the provided item ID.
    /// </summary>
    /// <param name="itemId">The ID of the item.</param>
    /// <returns>
    /// An <see cref="ItemHistory"/> object representing the item's history,
    /// or null if no history is found for the given item ID.
    /// </returns>
    public ItemHistory GetByItemId(string itemId)
    {
        return _collection.Query().Where(x => x.ItemId == itemId).Select(x => new ItemHistory
        {
            OrderId = x.OrderId,
            ItemId = x.ItemId,
            FoundDate = x.FoundDate
        }).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves the full ItemHistory by its itemId.
    /// </summary>
    /// <param name="itemId">The identifier of the item.</param>
    /// <returns>The full ItemHistory matching the itemId, or null if not found.</returns>
    public ItemHistory GetFullByItemId(string itemId)
    {
        return _collection.Query().Where(x => x.ItemId == itemId).FirstOrDefault();
    }

    /// <summary>
    /// Adds an <see cref="ItemHistory"/> entity to the collection.
    /// </summary>
    /// <param name="entity">The <see cref="ItemHistory"/> entity to add.</param>
    /// <returns>The added <see cref="ItemHistory"/> entity.</returns>
    public ItemHistory Add(ItemHistory entity)
    {
        _collection.Insert(entity);

        return entity;
    }

    /// <summary>
    /// Updates an item history entity in the collection.
    /// </summary>
    /// <param name="entity">The item history entity to be updated.</param>
    /// <returns>The updated item history entity.</returns>
    public ItemHistory Update(ItemHistory entity)
    {
        _collection.Update(entity.ItemId, entity);

        return entity;
    }

    /// <summary>
    /// Clears all the elements in the collection.
    /// </summary>
    /// <remarks>
    /// This method removes all the elements from the collection by deleting all the records in the database.
    /// </remarks>
    public void Clear()
    {
        _collection.DeleteAll();
    }

    /// <summary>
    /// Clears all items from the collection by given order ID.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    public void ClearByOrder(long orderId)
    {
        _collection.DeleteMany(x => x.OrderId == orderId);
    }
}