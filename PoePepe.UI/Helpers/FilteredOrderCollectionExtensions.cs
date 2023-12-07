using System.Collections.Generic;
using System.Linq;
using PoePepe.UI.Models;
using PoePepe.UI.ViewModels;

namespace PoePepe.UI.Helpers;

public static class FilteredOrderCollectionExtensions
{
    public static IOrderedEnumerable<OrderViewModel> Sort(this IEnumerable<OrderViewModel> orders, OrderSort sort)
    {
        return sort.Kind switch
        {
            OrderSortKind.NameAsc => orders.OrderBy(x => x.Name),
            OrderSortKind.NameDesc => orders.OrderByDescending(x => x.Name),
            OrderSortKind.CreationDateAsc => orders.OrderBy(x => x.CreatedAt),
            OrderSortKind.CreationDateDesc => orders.OrderByDescending(x => x.CreatedAt),
            OrderSortKind.Notify => orders.OrderByDescending(x => x.Mod),
            OrderSortKind.Whisper => orders.OrderBy(x => x.Mod),
            OrderSortKind.Enabled => orders.OrderBy(x => x.Activity),
            OrderSortKind.Disabled => orders.OrderByDescending(x => x.Activity),
            _ => orders.OrderByDescending(x => x.CreatedAt)
        };
    }
}