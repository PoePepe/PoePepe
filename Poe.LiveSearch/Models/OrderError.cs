namespace Poe.LiveSearch.Models;

public class OrderError
{
    public OrderError(long orderId, string errorMessage)
    {
        OrderId = orderId;
        ErrorMessage = errorMessage;
    }

    public long OrderId { get; set; }
    public string ErrorMessage { get; set; }
}

public class OrderError<T> : OrderError
{
    public T Content { get; set; }

    public OrderError(long orderId, string errorMessage, T content) : base(orderId, errorMessage)
    {
        Content = content;
    }
}
