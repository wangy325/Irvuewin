using System.Globalization;
using System.Windows.Data;

namespace Irvuewin.Helpers.Converters;

public class DateTimeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTimeOffset dateTime) return Binding.DoNothing;
        var prompt = "Next update";
        return dateTime.LocalDateTime > DateTimeOffset.Now
            ? string.Format(culture, "{0}: {1:yyyy-MM-dd HH:mm:ss}", prompt, dateTime)
            : $"Everything is OK!";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}