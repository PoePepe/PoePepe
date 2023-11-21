using Poe.UIW.Models;

namespace Poe.UIW.Services.ItemInfoTitle;

public class ResourceItemTitleService
{

    public static string GetItemTitleTextStyleName(OrderItemDto orderItem)
    {
        string styleName;
        switch (orderItem.ItemInfo.ItemFrameType)
        {
            case ItemFrameType.Magic:
                styleName = "itemName_Magic";
                
                break;
            
            case ItemFrameType.Rare:
                styleName = "itemName_Rare";
                
                break;
            
            case ItemFrameType.Unique:
                styleName = "itemName_Unique";
                
                break;
            
            case ItemFrameType.Gem:
                styleName = "itemName_Gem";
                break;
            
            case ItemFrameType.Special:
                styleName = "itemName_Currency";

                break;
            
            case ItemFrameType.FoilVariationRelic:
                styleName = "itemName_Foil_Rainbow";

                break;
            
            case ItemFrameType.FoilVariation:
                styleName = "itemName_Foil_Default";

                break;
            
            case ItemFrameType.Normal:
            case ItemFrameType.DivinationCard:
            case ItemFrameType.Other:
            default:
                styleName = "itemName_Normal";
                break;
        }

        return styleName;
    }

    public static ItemTitleImages GetItemTitleImages(OrderItemDto orderItem)
    {
        var itemTitleImages = new ItemTitleImages();
        switch (orderItem.ItemInfo.ItemFrameType)
        {
            case ItemFrameType.Normal:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleNormalLeftCachedBitmap;
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleNormalMiddleCachedBitmap;
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleNormalRightCachedBitmap;
                break;

            case ItemFrameType.Magic:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleMagicLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleMagicMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleMagicRightCachedBitmap;

                break;

            case ItemFrameType.Rare when orderItem.NameExists:

                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleDoubleRareLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleDoubleRareMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleDoubleRareRightCachedBitmap;

                break;
            
            case ItemFrameType.Rare:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleRareLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleRareMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleRareRightCachedBitmap;

                break;

            case ItemFrameType.Unique when orderItem.NameExists:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleDoubleUniqueLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleDoubleUniqueMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleDoubleUniqueRightCachedBitmap;

                break;
            
            case ItemFrameType.Unique :
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleUniqueLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleUniqueMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleUniqueRightCachedBitmap;

                break;

            case ItemFrameType.Gem:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleGemLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleGemMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleGemRightCachedBitmap;

                break;

            case ItemFrameType.FoilVariation when orderItem.NameExists:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleDoubleSupporterFoilLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap =
                    ItemTitleCacheStore.ItemTitleDoubleSupporterFoilMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap =
                    ItemTitleCacheStore.ItemTitleDoubleSupporterFoilRightCachedBitmap;

                break;
            
            case ItemFrameType.FoilVariation:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleSupporterFoilLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleSupporterFoilMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleSupporterFoilRightCachedBitmap;

                break;

            case ItemFrameType.FoilVariationRelic when orderItem.NameExists:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleDoubleRelicLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleDoubleRelicMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleDoubleRelicRightCachedBitmap;

                break;
            
            case ItemFrameType.FoilVariationRelic:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleRelicLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleRelicMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleRelicRightCachedBitmap;

                break;

            case ItemFrameType.Special:
            case ItemFrameType.DivinationCard:
            case ItemFrameType.Other:
            default:
                itemTitleImages.LeftImageBitmap = ItemTitleCacheStore.ItemTitleCurrencyLeftCachedBitmap;
                itemTitleImages.MiddleImageBitmap = ItemTitleCacheStore.ItemTitleCurrencyMiddleCachedBitmap;
                itemTitleImages.RightImageBitmap = ItemTitleCacheStore.ItemTitleCurrencyRightCachedBitmap;
                break;
        }

        return itemTitleImages;
    }
}