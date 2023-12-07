using System.Windows.Media.Imaging;

namespace PoePepe.UI.Services.Currency;

public class ResourceCurrencyService
{
    public static CachedBitmap GetCurrencyImage(string currencyName)
    {
        switch (currencyName)
        {
            case "chaos":
                return CurrencyCacheStore.ChaosOrbCachedBitmap;

            case "divine":
                return CurrencyCacheStore.DivineOrbCachedBitmap;

            case "gcp":
                return CurrencyCacheStore.GemcuttersPrismCachedBitmap;

            case "alt":
                return CurrencyCacheStore.AlterationCachedBitmap;

            case "alch":
                return CurrencyCacheStore.AlchemyCachedBitmap;

            case "vaal":
                return CurrencyCacheStore.VaalOrbCachedBitmap;

            case "regal":
                return CurrencyCacheStore.RegalOrbCachedBitmap;

            case "chance":
                return CurrencyCacheStore.ChanceOrbCachedBitmap;

            case "fusing":
                return CurrencyCacheStore.FusingOrbCachedBitmap;

            case "regret":
                return CurrencyCacheStore.RegretOrbCachedBitmap;

            case "scour":
                return CurrencyCacheStore.ScouringOrbCachedBitmap;

            case "jewellers":
                return CurrencyCacheStore.JewellersOrbCachedBitmap;

            case "chrome":
                return CurrencyCacheStore.ChromaticOrbCachedBitmap;

            case "chisel":
                return CurrencyCacheStore.CartographersChiselCachedBitmap;

            case "blessed":
                return CurrencyCacheStore.BlessedOrbBitmap;

            case "awakened-sextant":
                return CurrencyCacheStore.AwakenedSextantCachedBitmap;

            case "exalted":
                return CurrencyCacheStore.ExaltedOrdCachedBitmap;

            default:
                return CurrencyCacheStore.ChaosOrbCachedBitmap;
        }
    }
}