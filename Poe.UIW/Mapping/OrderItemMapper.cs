using System;
using System.Collections.Generic;
using System.Linq;
using Poe.LiveSearch.Api.Trade.Models;
using Poe.UIW.Models;
using LogbookMods = Poe.UIW.Models.LogbookMods;
using NotableProperty = Poe.UIW.Models.NotableProperty;
using Socket = Poe.UIW.Models.Socket;

namespace Poe.UIW.Mapping;

public static class OrderItemMapper
{
    public static OrderItemDto ToOrderItemDto(this FetchResponseResult response)
    {
        if (response is null)
        {
            return null;
        }

        var orderItem = new OrderItemDto
        {
            Id = response.Id,
            OrderId = response.OrderId,
            OrderName = response.OrderName,
            ImageUrl = response.Item.Icon,
            WhisperMessage = response.Listing.WhisperMessage,
            WhisperToken = response.Listing.WhisperToken,
            Price = new ItemPrice
            {
                Amount = response.Listing.Price.Amount,
                Currency = response.Listing.Price.Currency
            },
            NameExists = !string.IsNullOrEmpty(response.Item.Name),
            Name = response.Item.Name,
            TypeLine = response.Item.TypeLine,
            ItemKind = response.DetectItemType()
        };

        orderItem.ItemInfo = response.Item.ToItemInfo(orderItem.ItemKind);

        return orderItem;
    }

    private static string GetItemImageName(this FetchResponseResult response)
    {
        var lastSlashIndex = response.Item.Icon.LastIndexOf('/') + 1;
        return response.Item.Icon.Substring(lastSlashIndex);
    }

    private static ItemKind DetectItemType(this FetchResponseResult response)
    {
        if (response.Item.TypeLine.EndsWith("Resonator"))
        {
            return ItemKind.Resonator;
        }

        var imageName = response.GetItemImageName();

        if (imageName.StartsWith("Incubation"))
        {
            return ItemKind.Incubator;
        }

        if (imageName == "InventoryIcon.png")
        {
            return ItemKind.DivinationCard;
        }

        return ItemKind.Common;
    }

    private static string[] GetExplicitMods(string[] explicitMods, ItemKind itemKind)
    {
        if (itemKind == ItemKind.Incubator)
        {
            return explicitMods?.FirstOrDefault()?.Split("\r\n");
        }

        return explicitMods;
    }

    private static ItemFrameType ParseFrameType(int frameType)
    {
        if (Enum.IsDefined(typeof(ItemFrameType), frameType))
        {
            return (ItemFrameType)frameType;
        }

        return ItemFrameType.Other;
    }

    private static ItemInfo ToItemInfo(this Item item, ItemKind itemKind)
    {
        return new ItemInfo
        {
            ImageWidth = item.Width * 47,
            ItemFrameType = ParseFrameType(item.FrameType),
            FoilVariation = item.FoilVariation,

            HasItemLevel = item.ItemLevel > 0,
            ItemLevel = item.ItemLevel,

            Sockets = ToItemSockets(item),
            
            Properties = ToInfoProperties(item.Properties),
            ExtendedProperties = item.Extended?.ToExtendedProperties(),
            
            Requirements = item.Requirements?.Select(r => new ItemInfoRequirement
                { Name = r.Name, Value = r.Values?.FirstOrDefault()?.FirstOrDefault()?.ToString() }).ToArray(),
            
            ExplicitMods = GetExplicitMods(item.ExplicitMods, itemKind),
            ImplicitMods = item.ImplicitMods,

            EnchantMods = item.EnchantMods,
            NotableProperties = item.NotableProperties?.Select(x => new NotableProperty
            {
                Name = x.Name,
                Values = x.Values.Select(v => v.First().ToString()).ToArray()
            }).ToArray(),

            FracturedMods = item.FracturedMods,
            CraftedMods = item.CraftedMods,
            LogbookMods = item.LogbookMods?.Select(itemLogbookMod => new LogbookMods
                {
                    Name = itemLogbookMod.Name, FactionName = itemLogbookMod.Faction.Name, Mods = itemLogbookMod.Mods
                })
                .ToArray(),

            Influences = item.Influences?.Keys.ToArray(),
            IsIdentified = item.Identified,
            IsCorrupted = item.Corrupted,
            IsDuplicated = item.Duplicated,
            IsSplitted = item.Split,
            IsFractured = item.Fractured,
            IsSynthetic = item.Synthesised,
            IsReplica = item.Replica,
            IsVeiled = item.Veiled,
            IsSearing = item.Searing,
            IsTangled = item.Tangled,
            IsLogBook = item.LogbookMods?.Length > 0,
            ExistsNotableProperties = item.NotableProperties?.Length > 0,
            ExistsEnchantMods = item.EnchantMods?.Length > 0,
            ExistsImplicitMods = item.ImplicitMods?.Length > 0,
            ExistsExplicitMods = item.Identified && item.ExplicitMods?.Length > 0,
        };
    }

    private static ItemInfoProperty[] ToInfoProperties(Property[] properties)
    {
        if (properties is null || properties.Length == 0)
        {
            return Array.Empty<ItemInfoProperty>();
        }

        var infoProperties = new List<ItemInfoProperty>(properties.Length);
        foreach (var property in properties)
        {
            var infoProperty = new ItemInfoProperty();
            if (property.Name.Contains("{0}"))
            {
                infoProperty.Name = string.Format(property.Name, property.Values.Select(x => x.FirstOrDefault()).ToArray());
                infoProperty.NonValue = true;
                infoProperties.Add(infoProperty);
                continue;
            }

            infoProperty.Name = property.Name;
            infoProperty.Value = property.Values?.FirstOrDefault()?.FirstOrDefault()?.ToString();
            infoProperties.Add(infoProperty);
        }

        return infoProperties.ToArray();
    }

    private static Sockets ToItemSockets(Item item)
    {
        if (item.Sockets == null || item.Sockets.Count == 0)
        {
            return null;
        }
        
        var socketOrdinal = 1;

        return new Sockets
        {
            IsVertical = item.Width == 1,
            Count = item.Sockets.Count,
            Groups = item.Sockets
                .GroupBy(x => x.Group)
                .Select(x => new SocketGroup
                {
                    Sockets = x.Select(s => new Socket
                        {
                            OrdinalNumber = socketOrdinal++,
                            Color = GetSocketColor(s.Colour)
                        })
                        .ToArray()
                })
                .ToArray()
        };
    }

    private static SocketColor GetSocketColor(string rawColor)
    {
        return rawColor switch
        {
            "R" => SocketColor.Red,
            "G" => SocketColor.Green,
            "B" => SocketColor.Blue,
            "W" => SocketColor.White,
            "A" => SocketColor.Abyss,
            "DV" => SocketColor.Delve,
            _ => throw new ArgumentException("Unknown socket color.")
        };
    }

    private static ItemInfoExtendedProperty[] ToExtendedProperties(this Extended extended)
    {
        if (extended is null)
        {
            return Array.Empty<ItemInfoExtendedProperty>();
        }

        var result = new List<ItemInfoExtendedProperty>(3);

        if (extended.BaseDefencePercentile is not null)
        {
            result.Add(new ItemInfoExtendedProperty
            {
                Name = "Base Percentile: ",
                Value = $"{extended.BaseDefencePercentile}%"
            });
        }
        
        if(extended.Ar is not null)
        {
            result.Add(new ItemInfoExtendedProperty
            {
                Name = "Armour: ",
                Value = $"{extended.Ar}"
            });
        }
        
        if(extended.Ev is not null)
        {
            result.Add(new ItemInfoExtendedProperty
            {
                Name = "Evasion: ",
                Value = $"{extended.Ev}"
            });
        }

        if(extended.Es is not null)
        {
            result.Add(new ItemInfoExtendedProperty
            {
                Name = "Energy Shield: ",
                Value = $"{extended.Es}"
            });
        }
        
        if(extended.Ward is not null)
        {
            result.Add(new ItemInfoExtendedProperty
            {
                Name = "Ward: ",
                Value = $"{extended.Ward}"
            });
        }


        if(extended.Dps is not null)
        {
            result.Add(new ItemInfoExtendedProperty
            {
                Name = "Dps: ",
                Value = $"{extended.Dps}"
            });
        }

        if(extended.PDps is not null)
        {
            result.Add(new ItemInfoExtendedProperty
            {
                Name = "Physical Dps: ",
                Value = $"{extended.PDps}"
            });
        }

        if(extended.EDps is not null)
        {
            result.Add(new ItemInfoExtendedProperty
            {
                Name = "Elemental Dps: ",
                Value = $"{extended.EDps}"
            });
        }
        
        return result.ToArray();
    }
}