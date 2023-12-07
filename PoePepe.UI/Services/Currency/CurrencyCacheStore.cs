using System.Windows.Media.Imaging;

namespace PoePepe.UI.Services.Currency;

public static class CurrencyCacheStore
{
    public static readonly CachedBitmap AlchemyCachedBitmap = new(new BitmapImage(CurrencyImageLink.AlchemyOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap AlterationCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.AlterationOrbUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap AwakenedSextantCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.AwakenedSextantUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap BlessedOrbBitmap = new(new BitmapImage(CurrencyImageLink.BlessedOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap CartographersChiselCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.CartographersChiselUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap ChanceOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.ChanceOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap ChaosOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.ChaosOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap ChromaticOrbCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.ChromaticOrbUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap DivineOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.DivineOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap ExaltedOrdCachedBitmap = new(new BitmapImage(CurrencyImageLink.ExaltedOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap FusingOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.FusingOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap GemcuttersPrismCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.GemcuttersPrismUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap JewellersOrbCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.JewellersOrbUri), BitmapCreateOptions.None,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap RegalOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.RegalOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap RegretOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.RegretOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap ScouringOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.ScouringOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);

    public static readonly CachedBitmap VaalOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.VaalOrbUri),
        BitmapCreateOptions.None, BitmapCacheOption.Default);
}