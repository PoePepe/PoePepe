using System.Collections.Generic;
using Poe.LiveSearch.Models;
using Poe.UI.Models;
using Riok.Mapperly.Abstractions;

namespace Poe.UI.Mapping;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class OrderMapper2
{
    public static partial OrderDto2 ToOrderDto2(this Order car);
    public static partial Order ToOrder2(this OrderDto2 car);


    public static partial IEnumerable<OrderDto2> ToOrderDto2(this IEnumerable<Order> car);

}