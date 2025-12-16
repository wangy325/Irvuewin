using System.Globalization;
using System.Windows.Data;

namespace Irvuewin.Helpers.Converters;

public class DateTimeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTimeOffset dateTime) return Binding.DoNothing;
        return dateTime.LocalDateTime > DateTimeOffset.Now
            ? string.Format(culture, "{0} {1:yyyy-MM-dd HH:mm:ss}", Localization.Instance["Next_Update"], dateTime)
            : Localization.Instance["Everything_Is_OK"];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}