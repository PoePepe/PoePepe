using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace PoePepe.UI.Models
{

    public class TradeItem
    {
        public enum Currency { CHAOS, ALCHCHEMY, ALTERATION, ANCIENT, ANNULMENT, APPRENTICE_SEXTANT, ARMOUR_SCRAP, AUGMENTATION, BAUBLE, BESTIARY_ORB, BINDING_ORB, BLACKSMITH_WHETSTONE, BLESSING_CHAYULAH, BLESSING_ESH, BLESSING_TUL, BLESSING_UUL, BLESSING_XOPH, BLESSE, CHANCE, CHISEL, CHROM, DIVINE, ENGINEER, ETERNAL, EXALTED, FUSING, GEMCUTTERS, HARBINGER_ORB, HORIZON_ORB, IMPRINTED_BESTIARY, JEWELLER, JOURNEYMAN_SEXTANT, MASTER_SEXTANT, MIRROR, PORTAL, REGAL, REGRET, SCOUR, SILVER, SPLINTER_CHAYULA, SPLINTER_ESH, SPLINTER_TUL, SPLINTER_UUL, SPLINTER_XOPH, TRANSMUTE, VAAL, WISDOM, DIVINE_VESSEL, OFFERING_GODDESS, SACRIFICE_DAWN, SACRIFICE_DUSK, SACRIFICE_MIDNIGHT, SACRIFICE_NOON, PERANDUS_COIN }

        public enum TradeTypes { BUY, SELL }

        private static readonly List<TradeItem> lstTradeItems = new();
        private readonly Regex poeAppCurrencyRegex = new("@(.*) (.*): I'd like to buy your (.*) for my (.*) in (.*).(.*)");

        private readonly Regex poeAppRegEx = new("@(.*) (.*): wtb (.*) listed for (.*) in (.*) [(]stash \"(.*)[\"]; left ([0-9]*), top ([0-9]*)[)](.*)");
        private readonly Regex poeAppUnpricedRegex = new("@(.*) (.*): wtb (.*) in (.*) [(]stash \"(.*)[\"]; left ([0-9]*), top ([0-9]*)[)](.*)");
        private readonly Regex poeTradeCurrencyRegex = new("@(.*) (.*): Hi, I'd like to buy your (.*) for my (.*) in (.*).(.*)");
        private readonly Regex poeTradeNoLocationRegex = new("@(.*) (.*): Hi, I would like to buy your (.*) listed for (.*) in (.*)");

        private readonly Regex poeTradeRegex = new("@(.*) (.*): Hi, I would like to buy your (.*) listed for (.*) in (.*) [(]stash tab \"(.*)[\"]; position: left ([0-9]*), top ([0-9]*)[)](.*)");
        private readonly Regex poeTradeUnpricedRegex = new("@(.*) (.*): Hi, I would like to buy your (.*) in (.*) [(]stash tab \"(.*)[\"]; position: left ([0-9]*), top ([0-9]*)[)](.*)");


        // Constructor
        public TradeItem(string whisper)
        {
            WhisperMessage = whisper;

            SetTradeType(WhisperMessage);

            ParseWhisper(WhisperMessage);

            if (ItemExists(this))
            {
                throw new TradeItemExistsException("Item exists");
            }

            lstTradeItems.Add(this);
        }

        public TradeTypes TradeType
        {
            get;
            set;
        }

        public string Customer
        {
            get;
            set;
        }

        public string Item
        {
            get;
            set;
        }

        public Currency ItemCurrency
        {
            get;
            set;
        }

        public string ItemCurrencyQuant
        {
            get;
            set;
        }


        public string Price
        {
            get;
            set;
        }

        public string AdditionalText
        {
            get;
            set;
        }

        public string League { get; set; }

        public string Stash
        {
            get;
            set;
        }


        public string WhisperMessage
        {
            get;
            set;
        }

        public Currency PriceCurrency { get; set; }


        public bool ItemIsCurrency { get; set; }

        // Set property TradeType
        private void SetTradeType(string whisper)
        {
            if (whisper.Contains("@To"))
            {
                TradeType = TradeTypes.BUY;
            }
            else if (whisper.Contains("@From"))
            {
                TradeType = TradeTypes.SELL;
            }
        }

        private void ParseWhisper(string whisper)
        {

            if (poeTradeRegex.IsMatch(whisper))
            {
                var matches = Regex.Matches(whisper, poeTradeRegex.ToString());

                foreach (Match match in matches)
                {
                    // 
                    Customer = match.Groups[2].Value;

                    // Set customer
                    Customer = match.Groups[2].Value;

                    // Set item
                    Item = match.Groups[3].Value;

                    // Set price
                    Price = match.Groups[4].Value;

                    PriceCurrency = ParseCurrency(Price);


                    // Set league
                    League = match.Groups[5].Value;

                    // Set stash
                    Stash = match.Groups[6].Value;

                    // Set stash position

                    AdditionalText = match.Groups[9].Value;


                }
            }
            else if (poeTradeUnpricedRegex.IsMatch(whisper) && !whisper.Contains("listed for"))
            {
                var matches = Regex.Matches(whisper, poeTradeUnpricedRegex.ToString());

                foreach (Match match in matches)
                {
                    // Set customer
                    Customer = match.Groups[2].Value;

                    // Set item
                    Item = match.Groups[3].Value;

                    // Set league
                    League = match.Groups[4].Value;

                    // Set stash
                    Stash = match.Groups[5].Value;

                    // Set stash position

                    //this.AdditionalText = match.Groups[8].Value;
                }
            }
            else if (poeTradeNoLocationRegex.IsMatch(whisper))
            {
                var matches = Regex.Matches(whisper, poeTradeNoLocationRegex.ToString());

                foreach (Match match in matches)
                {
                    // 
                    Customer = match.Groups[2].Value;

                    // Set customer
                    Customer = match.Groups[2].Value;

                    // Set item
                    Item = match.Groups[3].Value;

                    // Set price
                    Price = match.Groups[4].Value;

                    PriceCurrency = ParseCurrency(Price);


                    // Set league
                    League = match.Groups[5].Value;

                    AdditionalText = match.Groups[9].Value;

                }

            }
            else if (poeTradeCurrencyRegex.IsMatch(whisper))
            {
                ItemIsCurrency = true;

                var matches = Regex.Matches(whisper, poeTradeCurrencyRegex.ToString());

                foreach (Match match in matches)
                {
                    // Set customer
                    Customer = match.Groups[2].Value;

                    // Set item
                    Item = match.Groups[3].Value;

                    // Set price
                    Price = match.Groups[4].Value;

                    try
                    {
                        PriceCurrency = ParseCurrency(Price);

                    }
                    catch (Exception)
                    {

                    }

                    //Catch ex. If ex thrown, the item could not be parsed. ItemIsCurrency will be set to false, so the item will be treated as normal item

                    try
                    {

                        ItemCurrencyQuant = ExtractPointNumberFromString(Item);

                        ItemCurrency = ParseCurrency(Item);

                    }
                    catch (Exception)
                    {
                        ItemIsCurrency = false;
                    }


                    // Set league
                    League = match.Groups[5].Value;

                    AdditionalText = match.Groups[6].Value;

                }


            }
            else if (poeAppRegEx.IsMatch(whisper))
            {
                var matches = Regex.Matches(whisper, poeAppRegEx.ToString());

                foreach (Match match in matches)
                {
                    // Set customer
                    Customer = match.Groups[2].Value;

                    // Set item
                    Item = match.Groups[3].Value;

                    // Set price
                    Price = match.Groups[4].Value;

                    PriceCurrency = ParseCurrency(Price);


                    // Set league
                    League = match.Groups[5].Value;

                    // Set stash
                    Stash = match.Groups[6].Value;

                    // Set stash position

                    AdditionalText = match.Groups[9].Value;
                }
            }
            else if (!whisper.Contains("listed for") && poeAppUnpricedRegex.IsMatch(whisper))
            {
                var matches = Regex.Matches(whisper, poeAppUnpricedRegex.ToString());

                foreach (Match match in matches)
                {
                    // Set customer
                    Customer = match.Groups[2].Value;

                    // Set item
                    Item = match.Groups[3].Value;

                    // Set league
                    League = match.Groups[4].Value;

                    // Set stash
                    Stash = match.Groups[5].Value;

                    // Set stash position

                    AdditionalText = match.Groups[8].Value;
                }
            }
            else if (!whisper.Contains("Hi, ") && poeAppCurrencyRegex.IsMatch(whisper))
            {
                ItemIsCurrency = true;

                var matches = Regex.Matches(whisper, poeAppCurrencyRegex.ToString());

                foreach (Match match in matches)
                {
                    // Set customer
                    Customer = match.Groups[2].Value;

                    // Set item
                    Item = match.Groups[3].Value;

                    // Set price
                    Price = match.Groups[4].Value;

                    PriceCurrency = ParseCurrency(Price);


                    try
                    {
                        ItemCurrencyQuant = ExtractPointNumberFromString(Item);

                        ItemCurrency = ParseCurrency(Item);

                    }
                    catch (Exception)
                    {
                        ItemIsCurrency = false;
                    }

                    // Set league
                    League = match.Groups[5].Value; ;

                    AdditionalText = match.Groups[6].Value;
                }


            }
            else
            {
                throw new NoRegExMatchException("No RegEx match for:\n" + whisper);
            }
        }

        private Currency ParseCurrency(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var strPrice = s.ToLower();

                if (strPrice.Contains("chaos") && !strPrice.Contains("shard"))
                {
                    return Currency.CHAOS;
                }

                if (strPrice.Contains("alch") && !strPrice.Contains("shard"))
                {
                    return Currency.ALCHCHEMY;
                }

                if (strPrice.Contains("alt") && !strPrice.Contains("ex"))
                {
                    return Currency.ALTERATION;
                }

                if (strPrice.Contains("ancient"))
                {
                    return Currency.ANCIENT;
                }

                if (strPrice.Contains("annulment") && !strPrice.Contains("shard"))
                {
                    return Currency.ANNULMENT;
                }

                if (strPrice.Contains("apprentice") && strPrice.Contains("sextant"))
                {
                    return Currency.APPRENTICE_SEXTANT;
                }

                if (strPrice.Contains("armour") || strPrice.Contains("scrap"))
                {
                    return Currency.ARMOUR_SCRAP;
                }

                if (strPrice.Contains("aug"))
                {
                    return Currency.AUGMENTATION;
                }

                if (strPrice.Contains("bauble"))
                {
                    return Currency.BAUBLE;
                }

                if (strPrice.Contains("bestiary") && strPrice.Contains("orb"))
                {
                    return Currency.BESTIARY_ORB;
                }

                if (strPrice.Contains("binding") && strPrice.Contains("orb"))
                {
                    return Currency.BINDING_ORB;
                }

                if (strPrice.Contains("whetstone") || strPrice.Contains("blacksmith"))
                {
                    return Currency.BLACKSMITH_WHETSTONE;
                }

                if (strPrice.Contains("blessing") && strPrice.Contains("chayulah"))
                {
                    return Currency.BLESSING_CHAYULAH;
                }

                if (strPrice.Contains("blessing") && strPrice.Contains("esh"))
                {
                    return Currency.BLESSING_ESH;
                }

                if (strPrice.Contains("blessing") && strPrice.Contains("tul"))
                {
                    return Currency.BLESSING_TUL;
                }

                if (strPrice.Contains("blessing") && strPrice.Contains("uul"))
                {
                    return Currency.BLESSING_UUL;
                }

                if (strPrice.Contains("blessing") && strPrice.Contains("xoph"))
                {
                    return Currency.BLESSING_XOPH;
                }

                if (strPrice.Contains("blesse"))
                {
                    return Currency.BLESSE;
                }

                if (strPrice.Contains("chance"))
                {
                    return Currency.CHANCE;
                }

                if (strPrice.Contains("chisel"))
                {
                    return Currency.CHISEL;
                }

                if (strPrice.Contains("chrom") || strPrice.Contains("chrome"))
                {
                    return Currency.CHROM;
                }

                if ((strPrice.Contains("divine") || strPrice.Contains("div")) && !strPrice.Contains("vessel"))
                {
                    return Currency.DIVINE;
                }

                if (strPrice.Contains("engineer") && strPrice.Contains("orb"))
                {
                    return Currency.ENGINEER;
                }

                if (strPrice.Contains("ete"))
                {
                    return Currency.ETERNAL;
                }

                if ((strPrice.Contains("ex") || strPrice.Contains("exa") || strPrice.Contains("exalted")) && !strPrice.Contains("shard") && !strPrice.Contains("sext"))
                {
                    return Currency.EXALTED;
                }

                if (strPrice.Contains("fuse") || strPrice.Contains("fus"))
                {
                    return Currency.FUSING;
                }

                if (strPrice.Contains("gcp") || strPrice.Contains("gemc"))
                {
                    return Currency.GEMCUTTERS;
                }

                if (strPrice.Contains("harbinger") && strPrice.Contains("orb"))
                {
                    return Currency.HARBINGER_ORB;
                }

                if (strPrice.Contains("horizon") && strPrice.Contains("orb"))
                {
                    return Currency.HORIZON_ORB;
                }

                if (strPrice.Contains("imprinted") && strPrice.Contains("bestiary"))
                {
                    return Currency.IMPRINTED_BESTIARY;
                }

                if (strPrice.Contains("jew"))
                {
                    return Currency.JEWELLER;
                }

                if (strPrice.Contains("journeyman") && strPrice.Contains("sextant"))
                {
                    return Currency.JOURNEYMAN_SEXTANT;
                }

                if (strPrice.Contains("master") && strPrice.Contains("sextant"))
                {
                    return Currency.MASTER_SEXTANT;
                }

                if (strPrice.Contains("mir") || strPrice.Contains("kal"))
                {
                    return Currency.MIRROR;
                }

                if (strPrice.Contains("coin"))
                {
                    return Currency.PERANDUS_COIN;
                }

                if (strPrice.Contains("port"))
                {
                    return Currency.PORTAL;
                }

                if (strPrice.Contains("rega"))
                {
                    return Currency.REGAL;
                }

                if (strPrice.Contains("regr"))
                {
                    return Currency.REGRET;
                }

                if (strPrice.Contains("dawn"))
                {
                    return Currency.SACRIFICE_DAWN;
                }

                if (strPrice.Contains("dusk"))
                {
                    return Currency.SACRIFICE_DUSK;
                }

                if (strPrice.Contains("midnight"))
                {
                    return Currency.SACRIFICE_MIDNIGHT;
                }

                if (strPrice.Contains("noon"))
                {
                    return Currency.SACRIFICE_NOON;
                }

                if (strPrice.Contains("scour"))
                {
                    return Currency.SCOUR;
                }

                if (strPrice.Contains("silver"))
                {
                    return Currency.SILVER;
                }

                if (strPrice.Contains("splinter") && strPrice.Contains("chayula"))
                {
                    return Currency.SPLINTER_CHAYULA;
                }

                if (strPrice.Contains("splinter") && strPrice.Contains("esh"))
                {
                    return Currency.SPLINTER_ESH;
                }

                if (strPrice.Contains("splinter") && strPrice.Contains("tul"))
                {
                    return Currency.SPLINTER_TUL;
                }

                if (strPrice.Contains("splinter") && strPrice.Contains("uul"))
                {
                    return Currency.SPLINTER_UUL;
                }

                if (strPrice.Contains("splinter") && strPrice.Contains("xoph"))
                {
                    return Currency.SPLINTER_XOPH;
                }

                if (strPrice.Contains("tra"))
                {
                    return Currency.TRANSMUTE;
                }

                if (strPrice.Contains("vaal"))
                {
                    return Currency.VAAL;
                }

                if (strPrice.Contains("wis"))
                {
                    return Currency.WISDOM;
                }

                if (strPrice.Contains("divine") && strPrice.Contains("vessel"))
                {
                    return Currency.DIVINE_VESSEL;
                }

                if (strPrice.Contains("offering") || strPrice.Contains("offer"))
                {
                    return Currency.OFFERING_GODDESS;
                }
            }
            throw new NoCurrencyFoundException("Currency " + s + " not found");
        }


        private static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH! Can not set first char to upper: " + input);
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        /// <summary>
        /// Returns the point number from a string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Point number if successfull, null if not</returns>
        public static string ExtractPointNumberFromString(string s)
        {
            var match = Regex.Match(s, @"([-+]?[0-9]*\.?[0-9]+)");

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        public static bool ItemExists(TradeItem ti)
        {
            foreach (var item in lstTradeItems)
            {
                if (item.Item == ti.Item && item.Customer == ti.Customer && item.Price == ti.Price && item.TradeType == ti.TradeType)
                {
                    return true;
                }
            }

            return false;

        }

        public static void RemoveItemFromList(TradeItem ti)
        {
            for (var i = 0; i < lstTradeItems.Count; i++)
            {
                if (lstTradeItems[i].Item == ti.Item && lstTradeItems[i].Customer == ti.Customer &&
                    lstTradeItems[i].Price == ti.Price
                    && lstTradeItems[i].TradeType == ti.TradeType)
                {
                    lstTradeItems.RemoveAt(i);
                }
            }
        }
    }

    [Serializable]
    internal class NoCurrencyBitmapFoundException : Exception
    {
        public NoCurrencyBitmapFoundException()
        {
        }

        public NoCurrencyBitmapFoundException(string message) : base(message)
        {
        }

        public NoCurrencyBitmapFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoCurrencyBitmapFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class NoCurrencyFoundException : Exception
    {
        public NoCurrencyFoundException()
        {
        }

        public NoCurrencyFoundException(string message) : base(message)
        {
        }

        public NoCurrencyFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoCurrencyFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class NoRegExMatchException : Exception
    {
        public NoRegExMatchException()
        {
        }

        public NoRegExMatchException(string message) : base(message)
        {
        }

        public NoRegExMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoRegExMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class TradeItemExistsException : Exception
    {
        public TradeItemExistsException()
        {
        }

        public TradeItemExistsException(string message) : base(message)
        {
        }

        public TradeItemExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TradeItemExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
