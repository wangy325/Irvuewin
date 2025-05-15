using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace Irvue_win.src.helper
{
    public static class TabItemHelper
    {
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.RegisterAttached(
                "IconSource",
                typeof(ImageSource),
                typeof(TabItemHelper),
                new FrameworkPropertyMetadata(null)
            );

        public static ImageSource GetIconSource(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(IconSourceProperty);
        }

        public static void SetIconSource(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(IconSourceProperty, value);
        }
    }

}
