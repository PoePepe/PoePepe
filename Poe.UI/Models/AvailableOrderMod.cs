using Poe.LiveSearch.Models;

namespace Poe.UI.Models;

public class AvailableOrderMod
{
    public AvailableOrderMod(OrderMod mod)
    {
        Mod = mod;
    }

    public OrderMod Mod { get; set; }
}