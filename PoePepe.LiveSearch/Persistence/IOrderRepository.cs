using PoePepe.LiveSearch.Models;

namespace PoePepe.LiveSearch.Persistence;

/// <summary>
/// Represents a repository for managing orders.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Adds an Order entity to the system.
    /// </summary>
    /// <param name="entity">The Order entity to be added.</param>
    /// <returns>The added Order entity.</returns>
    Order Add(Order entity);

    /// <summary>
    /// Update a specific order entity.
    /// </summary>
    /// <param name="entity">The order entity to update.</param>
    /// <returns>The updated order entity.</returns>
    Order Update(Order entity);

    /// <summary>
    /// Updates multiple orders with the specified modifications.
    /// </summary>
    /// <param name="orderIds">The list of order IDs to update.</param>
    /// <param name="mod">The order modifications to apply.</param>
    /// <remarks>
    /// This method updates multiple orders identified by their unique IDs
    /// with the specified modifications. The orderIds parameter is an
    /// enumerable collection of long integers representing the IDs of the
    /// orders to update. The mod parameter is an OrderMod object containing
    /// the modifications to apply to the orders.
    /// </remarks>
    void UpdateManyMod(IEnumerable<long> orderIds, OrderMod mod);

    /// <summary>
    /// Updates the specified OrderMod for all orders.
    /// </summary>
    /// <param name="mod">The OrderMod to update.</param>
    /// <remarks>
    /// This method updates the specified OrderMod for all orders. The OrderMod provides the updates to be applied to each order.
    /// </remarks>
    void UpdateAllMod(OrderMod mod);

    /// <summary>
    /// Deletes the record with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the record to be deleted.</param>
    /// <returns>True if the record was successfully deleted, otherwise false.</returns>
    bool Delete(long id);

    /// <summary>
    /// Clears all the data stored in the object.
    /// </summary>
    /// <remarks>
    /// This method removes all the data that has been stored in the object and resets it to its initial state.
    /// </remarks>
    void Clear();

    /// <summary>
    /// Retrieves all orders.
    /// </summary>
    /// <returns>
    /// An enumerable collection of Order objects.
    /// </returns>
    IEnumerable<Order> GetAll();

    /// <summary>
    /// Retrieves an order by its ID.
    /// </summary>
    /// <param name="id">The ID of the order to retrieve.</param>
    /// <returns>The order with the specified ID.</returns>
    Order GetById(long id);
}