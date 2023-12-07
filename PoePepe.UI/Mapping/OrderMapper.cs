using System.Collections.Generic;
using System.Linq;
using PoePepe.LiveSearch.Models;
using OrderViewModel = PoePepe.UI.ViewModels.OrderViewModel;

namespace PoePepe.UI.Mapping;

public static class OrderMapper
{
    public static OrderViewModel ToOrderModel(this Order order) =>
        new()
        {
            Id = order.Id,
            QueryHash = order.QueryHash,
            Name = order.Name,
            Activity = order.Activity,
            Mod = order.Mod,
            IsActive = order.Activity == OrderActivity.Enabled,
            CreatedAt = order.CreatedAt
        };

    public static Order ToOrder(this OrderViewModel order) =>
        new()
        {
            Id = order.Id,
            Name = order.Name,
            QueryHash = order.QueryHash,
            Activity = order.Activity,
            Mod = order.Mod,
            CreatedAt = order.CreatedAt
        };

    public static IEnumerable<OrderViewModel> ToOrderModel(this IEnumerable<Order> orders)
    {
        return orders.Select(x => x.ToOrderModel());
    }

    public static IEnumerable<Order> ToOrder(this IEnumerable<OrderViewModel> orders)
    {
        return orders.Select(x => x.ToOrder());
    }
}