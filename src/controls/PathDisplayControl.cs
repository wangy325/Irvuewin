using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Irvue_win.src.controls
{
    class PathDisplayControl : ContentControl
    {
        // 用于水平排列图标和文本
        private StackPanel? _panel;

        public static readonly DependencyProperty FullPathProperty =
            DependencyProperty.Register("FullPath", typeof(string), typeof(PathDisplayControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender,
                    OnFullPathChanged));

        public string FullPath
        {
            get { return (string)GetValue(FullPathProperty); }
            set { SetValue(FullPathProperty, value); }
        }

        static PathDisplayControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathDisplayControl), new FrameworkPropertyMetadata(typeof(PathDisplayControl)));
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

        public static void OnFullPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
            double contentWidth = this.ActualWidth == 0 ? 286 : this.ActualWidth;
            //Debug.WriteLine($"page width: {contentWidth}");

            string[] directories = FullPath.Split(System.IO.Path.DirectorySeparatorChar);

            // 取后半部分路径
            double _accumulatedWidth = 0;
            int _iconSize = 14;
           
            Stack<TextBlock> _stackBlocks = new();
            for (int i = (directories.Length - 1); i >= 0; i--)
            {
                string directory = directories[i];
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
                _accumulatedWidth += textBlock.ActualWidth + 6 + _iconSize;
                //Debug.WriteLine($"_accumulateWidth: {_accumulatedWidth}");


                if (_accumulatedWidth > contentWidth)
                    break;
                _stackBlocks.Push(textBlock);
            }

            //Debug.WriteLine($"block length: {_textBlocks.Count}");

            foreach (TextBlock item in _stackBlocks)
            {
                // Image不能复用~ （WPF不允许）
                Image icon = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Irvue-win;component/icons/settings/folder.ico")),
                    Width = _iconSize,
                    Height = _iconSize,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                _panel.Children.Add(icon);

                // 添加目录名
                _panel.Children.Add(item);
            }


            // 添加最后一个图标 (如果需要)
            //Image lastIcon = new Image();
            //lastIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Irvue-win;component/icons/settings/folder.ico")); 
            //lastIcon.Width = 16;
            //lastIcon.Height = 16;
            //_panel.Children.Add(lastIcon);
        }
    }

}
