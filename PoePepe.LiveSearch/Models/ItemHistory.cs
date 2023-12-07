using PoePepe.LiveSearch.Api.Trade.Models;

namespace PoePepe.LiveSearch.Models;

public class ItemHistory
{
    public long OrderId { get; set; }
    public string ItemId { get; set; }
    public DateTimeOffset FoundDate { get; set; }
    public FetchResponseResult ItemData { get; set; }
}