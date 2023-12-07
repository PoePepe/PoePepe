namespace PoePepe.LiveSearch.WebSocket;

public class ItemLiveResponse
{
    public ItemLiveResponse(string itemId, long orderId, string orderName)
    {
        ItemId = itemId;
        OrderId = orderId;
        OrderName = orderName;
    }

    public string ItemId { get; set; }

    public long OrderId { get; set; }
    public string OrderName { get; set; }
}