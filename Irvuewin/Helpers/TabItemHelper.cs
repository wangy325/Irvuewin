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
    }

}
