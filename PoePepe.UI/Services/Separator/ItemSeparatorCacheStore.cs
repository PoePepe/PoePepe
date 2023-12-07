using System.Windows.Media.Imaging;

namespace PoePepe.UI.Services.Separator;

public static class ItemSeparatorCacheStore
{
    public static readonly CachedBitmap ItemSeparatorCurrencyCachedBitmap =
        new(new BitmapImage(ItemSeparatorImageLink.ItemSeparatorCurrencyUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap ItemSeparatorGemCachedBitmap = new(
        new BitmapImage(ItemSeparatorImageLink.ItemSeparatorGemUri), BitmapCreateOptions.None,
        BitmapCacheOption.Default);

    public static readonly CachedBitmap ItemSeparatorMagicCachedBitmap = new(
        new BitmapImage(ItemSeparatorImageLink.ItemSeparatorMagicUri), BitmapCreateOptions.None,
        BitmapCacheOption.Default);

    public static readonly CachedBitmap ItemSeparatorNormalCachedBitmap = new(
        new BitmapImage(ItemSeparatorImageLink.ItemSeparatorNormalUri), BitmapCreateOptions.None,
        BitmapCacheOption.Default);

    public static readonly CachedBitmap ItemSeparatorRareCachedBitmap = new(
        new BitmapImage(ItemSeparatorImageLink.ItemSeparatorRareUri), BitmapCreateOptions.None,
        BitmapCacheOption.Default);

    public static readonly CachedBitmap ItemSeparatorUniqueCachedBitmap = new(
        new BitmapImage(ItemSeparatorImageLink.ItemSeparatorUniqueUri), BitmapCreateOptions.None,
        BitmapCacheOption.Default);

    public static readonly CachedBitmap ItemSeparatorRelicCachedBitmap = new(
        new BitmapImage(ItemSeparatorImageLink.ItemSeparatorRelicUri), BitmapCreateOptions.None,
        BitmapCacheOption.Default);

    public static readonly CachedBitmap ItemSeparatorSupporterFoilCachedBitmap = new(
        new BitmapImage(ItemSeparatorImageLink.ItemSeparatorSupporterFoilUri), BitmapCreateOptions.None,
        BitmapCacheOption.Default);
}