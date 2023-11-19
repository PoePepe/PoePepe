using System.Windows.Media.Imaging;

namespace Poe.UIW.Services.Currency;

public static class CurrencyCacheStore
{
    public static readonly CachedBitmap AlchemyCachedBitmap = new(new BitmapImage(CurrencyImageLink.AlchemyOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap AlterationCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.AlterationOrbUri), BitmapCreateOptions.DelayCreation,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap AwakenedSextantCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.AwakenedSextantUri), BitmapCreateOptions.DelayCreation,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap BlessedOrbBitmap = new(new BitmapImage(CurrencyImageLink.BlessedOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap CartographersChiselCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.CartographersChiselUri), BitmapCreateOptions.DelayCreation,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap ChanceOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.ChanceOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap ChaosOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.ChaosOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap ChromaticOrbCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.ChromaticOrbUri), BitmapCreateOptions.DelayCreation,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap DivineOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.DivineOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap ExaltedOrdCachedBitmap = new(new BitmapImage(CurrencyImageLink.ExaltedOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap FusingOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.FusingOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap GemcuttersPrismCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.GemcuttersPrismUri), BitmapCreateOptions.DelayCreation,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap JewellersOrbCachedBitmap =
        new(new BitmapImage(CurrencyImageLink.JewellersOrbUri), BitmapCreateOptions.DelayCreation,
            BitmapCacheOption.Default);

    public static readonly CachedBitmap RegalOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.RegalOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap RegretOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.RegretOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap ScouringOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.ScouringOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);

    public static readonly CachedBitmap VaalOrbCachedBitmap = new(new BitmapImage(CurrencyImageLink.VaalOrbUri),
        BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);
}