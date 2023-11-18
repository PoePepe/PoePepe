namespace Poe.LiveSearch.Models;

public class OrderBaseInfo
{
    public OrderBaseInfo(long orderId, string orderName)
    {
        OrderId = orderId;
        OrderName = orderName;
    }
    public long OrderId { get; set; }
    public string OrderName { get; set; }
}