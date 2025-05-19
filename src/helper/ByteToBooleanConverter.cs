using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Irvue_win.src.helper
{
    class ByteToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte byteValue && parameter is string parameterString && byte.TryParse(parameterString, out byte parameterByte))
            {
                return byteValue == parameterByte;
            }
            return false; // 默认返回 false
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string parameterString && byte.TryParse(parameterString, out byte parameterByte))
            {
                // 如果选中，返回 parameterByte，否则返回 0 (或者其他默认值)
                return boolValue ? parameterByte : (byte)0; 
            }
            return (byte)0; // 默认返回 0
        }
    }
}
