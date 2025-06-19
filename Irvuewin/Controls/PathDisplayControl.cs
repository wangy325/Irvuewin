using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Irvuewin.Controls
{
    public class PathDisplayControl : ContentControl
    {
        // 用于水平排列图标和文本
        private StackPanel? _panel;

        public static readonly DependencyProperty FullPathProperty =
            DependencyProperty.Register(
                nameof(FullPath),
                typeof(string),
                typeof(PathDisplayControl),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnFullPathChanged));

        public string FullPath
        {
            get => (string)GetValue(FullPathProperty);
            set => SetValue(FullPathProperty, value);
        }

        static PathDisplayControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PathDisplayControl),
                new FrameworkPropertyMetadata(typeof(PathDisplayControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _panel = Template.FindName("PART_Panel", this) as StackPanel;
            if (_panel == null)
            {
                _panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                Content = _panel;
            }

            UpdateDisplay();
        }

        private static void OnFullPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PathDisplayControl control = (PathDisplayControl)d;
            control.UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_panel == null) return;
            // 清空之前的元素
            _panel.Children.Clear();

            if (string.IsNullOrEmpty(FullPath)) return;

            // page width
            // TODO: 首次打开窗口读数为0
            var contentWidth = ActualWidth == 0 ? 286 : ActualWidth;
            //Debug.WriteLine($"page width: {contentWidth}");
            var directories = FullPath.Split(System.IO.Path.DirectorySeparatorChar);

            // 取后半部分路径
            double accumulatedWidth = 0;
            const int iconSize = 14;

            Stack<TextBlock> stackBlocks = new();
            for (var i = (directories.Length - 1); i >= 0; i--)
            {
                var directory = directories[i];
                if (string.IsNullOrEmpty(directory)) continue;

                // 添加目录名
                TextBlock textBlock = new()
                {
                    Text = directory,
                    Margin = new Thickness(3, 0, 3, 0),
                    //FontFamily = new FontFamily("YaHei UI"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    //FontSize = 14,
                };
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                textBlock.Arrange(new Rect(textBlock.DesiredSize));

                // magic number is margin
                accumulatedWidth += textBlock.ActualWidth + 6 + iconSize;
                //Debug.WriteLine($"_accumulateWidth: {_accumulatedWidth}");

                if (accumulatedWidth > contentWidth)
                    break;
                stackBlocks.Push(textBlock);
            }

            //Debug.WriteLine($"block length: {_textBlocks.Count}");

            foreach (var item in stackBlocks)
            {
                // Image不能复用~ （WPF不允许）
                Image icon = new()
                {
                    Source = new BitmapImage(
                        new Uri("pack://application:,,,/Irvuewin;component/icons/settings/folder.ico")),
                    Width = iconSize,
                    Height = iconSize,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                _panel.Children.Add(icon);
                // 添加目录名
                _panel.Children.Add(item);
            }
        }
    }
}