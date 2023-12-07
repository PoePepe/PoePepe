using System.Linq;
using System.Windows.Media.Imaging;
using PoePepe.UI.Models;

namespace PoePepe.UI.Services.Influence;

public class ResourceItemInfluenceService
{
    public static bool TryGetItemInfluenceImage(OrderItemDto orderItem, out ItemInfluencesImages images)
    {
        var itemInfo = orderItem.ItemInfo;
        if (itemInfo.IsFractured)
        {
            images = new ItemInfluencesImages
            {
                LeftImageBitmap = ItemInfluenceCacheStore.FracturedSymbolCachedBitmap,
                RightImageBitmap = ItemInfluenceCacheStore.FracturedSymbolCachedBitmap
            };

            return true;
        }

        if (itemInfo.IsSynthetic)
        {
            images = new ItemInfluencesImages
            {
                LeftImageBitmap = ItemInfluenceCacheStore.SyntheticSymbolCachedBitmap,
                RightImageBitmap = ItemInfluenceCacheStore.SyntheticSymbolCachedBitmap
            };
            return true;
        }

        if (itemInfo.IsSearing || itemInfo.IsTangled)
        {
            images = new ItemInfluencesImages
            {
                LeftImageBitmap = itemInfo.IsSearing
                    ? ItemInfluenceCacheStore.SearingSymbolCachedBitmap
                    : ItemInfluenceCacheStore.TangledSymbolCachedBitmap,
                RightImageBitmap =
                    itemInfo.IsTangled
                        ? ItemInfluenceCacheStore.TangledSymbolCachedBitmap
                        : ItemInfluenceCacheStore.SearingSymbolCachedBitmap
            };
            return true;
        }

        if (itemInfo.IsReplica)
        {
            images = new ItemInfluencesImages
            {
                LeftImageBitmap = ItemInfluenceCacheStore.ExperimentedSymbolCachedBitmap,
                RightImageBitmap = ItemInfluenceCacheStore.ExperimentedSymbolCachedBitmap
            };
            return true;
        }

        if (itemInfo.Influences is not null && itemInfo.Influences.Any())
        {
            if (TryGetInfluenceImage(itemInfo.Influences.First(), out var left))
            {
                images = new ItemInfluencesImages
                {
                    LeftImageBitmap = left
                };

                if (itemInfo.Influences.Length > 1 && TryGetInfluenceImage(itemInfo.Influences[1], out var right))
                {
                    images.RightImageBitmap = right;
                    return true;
                }

                images.RightImageBitmap = left;
                return true;
            }
        }

        if (itemInfo.IsVeiled)
        {
            images = new ItemInfluencesImages
            {
                LeftImageBitmap = ItemInfluenceCacheStore.VeiledSymbolCachedBitmap,
                RightImageBitmap = ItemInfluenceCacheStore.VeiledSymbolCachedBitmap
            };
            return true;
        }

        images = null;
        return false;
    }

    private static bool TryGetInfluenceImage(string name, out CachedBitmap result)
    {
        switch (name)
        {
            case "shaper":
                result = ItemInfluenceCacheStore.ShaperSymbolCachedBitmap;
                return true;
            case "hunter":
                result = ItemInfluenceCacheStore.HunterSymbolCachedBitmap;
                return true;
            case "elder":
                result = ItemInfluenceCacheStore.ElderSymbolCachedBitmap;
                return true;
            case "warlord":
                result = ItemInfluenceCacheStore.WarlordSymbolCachedBitmap;
                return true;
            case "crusader":
                result = ItemInfluenceCacheStore.CrusaderSymbolCachedBitmap;
                return true;
            case "redeemer":
                result = ItemInfluenceCacheStore.RedeemerSymbolCachedBitmap;
                return true;
            default:
                result = null;
                return false;
        }
    }
}