using PoePepe.LiveSearch.Models;

namespace PoePepe.LiveSearch.Persistence;

/// <summary>
/// Represents a repository for managing item history entities.
/// </summary>
public interface IItemHistoryRepository
{
    /// <summary>
    /// Adds an ItemHistory entity to the system.
    /// </summary>
    /// <param name="entity">The ItemHistory entity to be added.</param>
    /// <returns>The added ItemHistory entity.</returns>
    ItemHistory Add(ItemHistory entity);

    /// <summary>
    /// Updates an ItemHistory entity.
    /// </summary>
    /// <param name="entity">The ItemHistory entity to update.</param>
    /// <returns>The updated ItemHistory entity.</returns>
    ItemHistory Update(ItemHistory entity);

    /// <summary>
    /// Clears the content of the current object.
    /// </summary>
    void Clear();

    /// <summary>
    /// Clears the order identified by the specified ID.
    /// </summary>
    /// <param name="orderId">The unique identifier for the order to be cleared.</param>
    void ClearByOrder(long orderId);

    /// <summary>
    /// Retrieves a collection of ItemHistory objects by order ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve ItemHistory objects for.</param>
    /// <param name="top">The number of ItemHistory objects to retrieve (top N).</param>
    /// <param name="skip">The number of ItemHistory objects to skip (for pagination).</param>
    /// <returns>A collection of ItemHistory objects that match the specified order ID.</returns>
    IEnumerable<ItemHistory> GetByOrderId(long orderId, int top, int skip);

    /// <summary>
    /// Retrieves an ItemHistory object by its itemId.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <returns>The ItemHistory object associated with the itemId.</returns>
    ItemHistory GetByItemId(string itemId);

    /// <summary>
    /// Retrieves the full item history for a given item ID.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <returns>The full item history as an ItemHistory object.</returns>
    ItemHistory GetFullByItemId(string itemId);
}