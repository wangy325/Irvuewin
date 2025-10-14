using System.Globalization;
using System.Windows.Data;

namespace Irvuewin.Helpers.Converters
{
    public class FloatToStringConverter : IValueConverter
    {
        // source -> target
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)       
        {
            if (value is float percentage )
            {
                return $"{percentage:P0}";
            }
            return string.Empty;
        }


        // target -> source
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && float.TryParse(str.Trim('%'), out var result))
            {
                return result / 100f;
            }
            return Binding.DoNothing; 
        }
    }
}
