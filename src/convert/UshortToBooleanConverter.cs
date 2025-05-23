using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Irvuewin.src.convert

{
    /// <summary>
    /// convert wallpaperChangeuIntervals checked tag to ushort
    /// settings item: WallpaperChangeInterval
    ///     0 - manual
    ///     10 - 10 min
    ///     30 - 30 min
    ///     60 - 1 hour
    ///     120 - 2 hours
    ///     180 - 3 hours
    ///     1440 - 1 day
    class UshortToBooleanConverter : IValueConverter
    {
       

        // source -> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ushort val && parameter is string p && ushort.TryParse(p, out ushort res))
            {
                return val == res;
            }
            return false;
        }

        // target -> source 
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && parameter is string p && ushort.TryParse(p, out ushort res))
            {
                // 如果选中，返回 转换参数的值，否则返回 0 
                return isChecked ? res : (byte)0;
            }
            return (byte)0;
        }
    }
}
