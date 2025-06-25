using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Irvuewin.Helpers.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case null:
            case string str when string.IsNullOrWhiteSpace(str):
                return Visibility.Collapsed;
            default:
                return Visibility.Visible;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}