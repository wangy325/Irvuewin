using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

            string[] directories = FullPath.Split(Path.DirectorySeparatorChar);

            // 取后半部分路径
            for (int i = (directories.Length - 3); i < directories.Length; i++)
            {
                string directory = directories[i];
                if (string.IsNullOrEmpty(directory)) continue;

                // 添加图标
                Image icon = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Irvue-win;component/icons/settings/folder.ico")),
                    Width = 14,
                    Height = 14,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                _panel.Children.Add(icon);

                // 添加目录名
                TextBlock textBlock = new()
                {
                    Text = directory,
                    Margin = new Thickness(5, 0, 5, 0),
                    FontFamily = new System.Windows.Media.FontFamily("Cascadia Code"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    //FontSize = 14,
                };

                _panel.Children.Add(textBlock);
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
