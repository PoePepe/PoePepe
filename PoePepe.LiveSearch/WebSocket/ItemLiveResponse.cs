namespace PoePepe.LiveSearch.WebSocket;

/// <summary>
/// Represents a response containing live information about an item.
/// </summary>
public class ItemLiveResponse
{
    public ItemLiveResponse(string itemId, long orderId, string orderName)
    {
        ItemId = itemId;
        OrderId = orderId;
        OrderName = orderName;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the item.
    /// </summary>
    /// <value>
    /// The unique identifier of the item.
    /// </value>
    public string ItemId { get; set; }

    /// <summary>
    /// Gets or sets the OrderId of the order.
    /// </summary>
    /// <value>The OrderId as a long.</value>
    public long OrderId { get; set; }

    /// <summary>
    /// Gets or sets the name of the order.
    /// </summary>
    /// <value>
    /// The name of the order.
    /// </value>
    public string OrderName { get; set; }
}