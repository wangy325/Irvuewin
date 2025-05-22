using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Irvue_win.src.convert
{
    class FloatToStringConverter : IValueConverter
    {
        // source -> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float percentage )
            {
                return $"{percentage:P0}";
            }
            //Debug.WriteLine($"Convert: {value}");
            return string.Empty;
        }


        // target -> source
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && float.TryParse(str.Trim('%'), out float result))
            {
                //Debug.WriteLine($"ConvertBack: {result/100f}");
                return result / 100f;
            }
            return 1.0f; 
        }
    }
}
