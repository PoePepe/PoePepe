using System.Windows.Media.Imaging;
using PoePepe.UI.Models;

namespace PoePepe.UI.Services.Separator;

public class ResourceItemSeparatorService
{
    public static CachedBitmap GetItemSeparatorImage(OrderItemDto orderItem)
    {
        switch (orderItem.ItemInfo.ItemFrameType)
        {
            case ItemFrameType.Normal:
                return ItemSeparatorCacheStore.ItemSeparatorNormalCachedBitmap;

            case ItemFrameType.Magic:
                return ItemSeparatorCacheStore.ItemSeparatorMagicCachedBitmap;

            case ItemFrameType.Rare:
                return ItemSeparatorCacheStore.ItemSeparatorRareCachedBitmap;

            case ItemFrameType.Unique:
                return ItemSeparatorCacheStore.ItemSeparatorUniqueCachedBitmap;

            case ItemFrameType.Gem:
                return ItemSeparatorCacheStore.ItemSeparatorGemCachedBitmap;

            case ItemFrameType.FoilVariation:
                return ItemSeparatorCacheStore.ItemSeparatorSupporterFoilCachedBitmap;

            case ItemFrameType.FoilVariationRelic:
                return ItemSeparatorCacheStore.ItemSeparatorRelicCachedBitmap;

            case ItemFrameType.Special:
            case ItemFrameType.DivinationCard:
            case ItemFrameType.Other:
            default:
                return ItemSeparatorCacheStore.ItemSeparatorCurrencyCachedBitmap;
        }
    }
}