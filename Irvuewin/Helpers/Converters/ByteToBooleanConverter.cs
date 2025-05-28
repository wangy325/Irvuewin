using System.Globalization;
using System.Windows.Data;

namespace Irvuewin.Helpers.Converters
{
    public class ByteToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// source -> target
        /// </summary>
        /// <param name="value">source value</param>
        /// <param name="targetType">The type of the binding target property.(target type)</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not byte val || parameter is not string p || !byte.TryParse(p, out var res))
                return false;
            System.Diagnostics.Debug.WriteLine($"source -> target: val: {val}, res: {res}, p: {p}");
            return val == res;
        }

        /// <summary>
        /// target -> source
        /// </summary>
        /// <param name="value">target value</param>
        /// <param name="targetType"> The type to convert to.(source type)</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool val || parameter is not string p || !byte.TryParse(p, out var res))
                return Binding.DoNothing;
            System.Diagnostics.Debug.WriteLine($"target -> source: val: {val}, p: {p}, res: {res}");
            // 如果选中，返回 转换参数的值（byte），否则返回 0 
            return val ? res : Binding.DoNothing;
        }
    }
}
