using System.Windows.Media.Imaging;

namespace Poe.UIW.Services.Influence;

public static class ItemInfluenceCacheStore
{
    public static readonly CachedBitmap CrusaderSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.CrusaderSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap ElderSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.ElderSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap ExperimentedSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.ExperimentedSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap FracturedSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.FracturedSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap HunterSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.HunterSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap RedeemerSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.RedeemerSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap SearingSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.SearingSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap ShaperSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.ShaperSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap SyntheticSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.SyntheticSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap TangledSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.TangledSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap VeiledSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.VeiledSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap WarlordSymbolCachedBitmap =
        new(new BitmapImage(ItemInfluenceImageLink.WarlordSymbolUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);
}