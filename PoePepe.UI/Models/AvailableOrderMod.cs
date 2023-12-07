using PoePepe.LiveSearch.Models;

namespace PoePepe.UI.Models;

public class AvailableOrderMod
{
    public AvailableOrderMod(OrderMod mod)
    {
        Mod = mod;
    }

    public OrderMod Mod { get; set; }
}