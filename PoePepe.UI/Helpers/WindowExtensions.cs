using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PoePepe.UI.Helpers;

public static class WindowExtensions
{
    public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null)
        {
            yield return (T)Enumerable.Empty<T>();
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var ithChild = VisualTreeHelper.GetChild(depObj, i);
            if (ithChild is T t) yield return t;
            foreach (var childOfChild in FindVisualChildren<T>(ithChild)) yield return childOfChild;
        }
    }
}