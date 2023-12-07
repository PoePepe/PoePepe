using System;
using System.Globalization;
using System.Windows.Data;
using PoePepe.UI.Services.Currency;

namespace PoePepe.UI.Helpers;

public class CurrencyImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not string valueString ? value : ResourceCurrencyService.GetCurrencyImage(valueString);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}