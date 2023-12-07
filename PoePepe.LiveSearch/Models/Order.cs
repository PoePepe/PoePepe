namespace PoePepe.LiveSearch.Models;

public class Order
{
    public long Id { get; set; }
    public string QueryHash { get; set; }
    public string Name { get; set; }
    public string QueryLink { get; set; }
    public OrderActivity Activity { get; set; }
    public OrderMod Mod { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}