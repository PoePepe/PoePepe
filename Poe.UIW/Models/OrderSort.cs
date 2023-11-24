namespace Poe.UIW.Models;

public class OrderSort
{
    public OrderSort(OrderSortKind kind, string description)
    {
        Kind = kind;
        Description = description;
    }

    public OrderSortKind Kind { get; set; }
    public string Description { get; set; }
}