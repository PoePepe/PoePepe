using System.Windows;
using ValueConverters;

namespace PoePepe.UI.Helpers;

public class BoolToHiddenVisibilityConverter : BoolToValueConverter<Visibility>
{
    public BoolToHiddenVisibilityConverter()
    {
        TrueValue = Visibility.Visible;
        FalseValue = Visibility.Hidden;
    }
}