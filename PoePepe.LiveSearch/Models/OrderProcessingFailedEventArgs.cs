namespace PoePepe.LiveSearch.Models;

public class OrderProcessingFailedEventArgs : EventArgs
{
    public OrderProcessingFailedEventArgs(long orderId)
    {
        OrderId = orderId;
    }

    public long OrderId { get; set; }
}