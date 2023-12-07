namespace PoePepe.LiveSearch.Models;

public class OrderError
{
    public OrderError(long orderId, string errorMessage, OrderErrorType errorType = OrderErrorType.Connection)
    {
        OrderId = orderId;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public long OrderId { get; set; }
    public string ErrorMessage { get; set; }
    public OrderErrorType ErrorType { get; set; }
}

public enum OrderErrorType
{
    Connection,
    Process
}

public class OrderError<T> : OrderError
{
    public OrderError(long orderId, string errorMessage, T content) : base(orderId, errorMessage)
    {
        Content = content;
    }

    public T Content { get; set; }
}
