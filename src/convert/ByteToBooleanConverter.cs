using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Irvue_win.src.convert
{
    class ByteToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// source -> target
        /// </summary>
        /// <param name="value">source value</param>
        /// <param name="targetType">The type of the binding target property.(target type)</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte val && parameter is string p && byte.TryParse(p, out byte res))
            {
                System.Diagnostics.Debug.WriteLine($"source -> target: val: {val}, res: {res}, p: {p}");
                return val == res;
            }
            return false; // 默认返回 false
        }

        /// <summary>
        /// target -> source
        /// </summary>
        /// <param name="value">target value</param>
        /// <param name="targetType"> The type to convert to.(source type)</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool val && parameter is string p && byte.TryParse(p, out byte res))
            {
                System.Diagnostics.Debug.WriteLine($"target -> source: val: {val}, p: {p}, res: {res}");
                // 如果选中，返回 转换参数的值（byte），否则返回 0 
                return val ? res : Binding.DoNothing; 
            }
            return Binding.DoNothing;
        }
    }
}
