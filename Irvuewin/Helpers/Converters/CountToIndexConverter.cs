using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Irvuewin.Helpers.Converters;

public class CountToIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // return value is int count ? count - 1 : Binding.DoNothing;
        
        
        /*if (parameter is IEnumerable itemsSource && value is DependencyObject item)
        {
            var items = itemsSource.Cast<object>().ToList();
            if (items.Count > 0 && ItemsControl.ItemsControlFromItemContainer(item) is ItemsControl itemsControl)
            {
                return ReferenceEquals(item, itemsControl.Items[itemsControl.Items.Count - 1]);
            }
        }
        return false;*/
        return value is int and 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}