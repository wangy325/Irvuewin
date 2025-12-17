using System.Windows;
using System.Windows.Media;

namespace Irvuewin.Helpers
{
    // 设置页面TabItem图标文件帮助类
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

        public static readonly DependencyProperty IconGeometryProperty =
            DependencyProperty.RegisterAttached(
                "IconGeometry",
                typeof(Geometry),
                typeof(TabItemHelper),
                new FrameworkPropertyMetadata(null)
            );

        public static Geometry GetIconGeometry(DependencyObject obj)
        {
            return (Geometry)obj.GetValue(IconGeometryProperty);
        }

        public static void SetIconGeometry(DependencyObject obj, Geometry value)
        {
            obj.SetValue(IconGeometryProperty, value);
        }
    }

}
