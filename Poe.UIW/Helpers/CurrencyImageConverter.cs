using System;
using System.Globalization;
using System.Windows.Data;
using Poe.UIW.Services.Currency;

namespace Poe.UIW.Helpers;

public class CurrencyImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not string valueString ? value : ResourceCurrencyService.GetCurrencyImage(valueString);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}