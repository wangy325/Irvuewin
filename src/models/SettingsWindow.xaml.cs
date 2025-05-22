using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using Irvue_win.Properties;
using Irvue_win.src.controls;

namespace Irvue_win.src.models
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }


        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 加载保存的位置和大小
            this.Left = Settings.Default.SettingsWindowLeft;
            this.Top = Settings.Default.SettingsWindowTop;

            // 防止窗口初始启动时不在屏幕内
            if (this.Left < 0 || this.Top < 0 || this.Left > SystemParameters.VirtualScreenWidth - this.Width || this.Top > SystemParameters.VirtualScreenHeight - this.Height)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 保存窗口的位置和大小
            Settings.Default.SettingsWindowLeft = this.Left;
            Settings.Default.SettingsWindowTop = this.Top;
            Settings.Default.Save();
        }


        private void BrowseWallpaperSavePathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                Title = "壁纸保存文件夹",
                IsFolderPicker = true,
                InitialDirectory = $"C:\\Users\\{Environment.UserName}\\Pictures",
                ShowHiddenItems = false,
            };

            // 指定 owner，防止任务栏出现新图标
            var result = dialog.ShowDialog(this);

            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                string? selectedPath = dialog.FileName;
                if (selectedPath != null)
                {
                    var systemFolders = new[]
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                        Environment.GetFolderPath(Environment.SpecialFolder.System),
                    };

                    if (systemFolders.Any(sf => selectedPath.StartsWith(sf, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("不能选择系统文件夹，请选择其他文件夹。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    // WallpaperSavePathTextBox.Text = selectedPath;

                    // 获取 PathDisplayControl 的 BindingExpression
                    // 更新回显
                    PathDisplayControl pathDisplayControl = (PathDisplayControl)this.FindName("PathDisplayControl");
                    //Debug.WriteLine($"====>{pathDisplayControl}");
                    pathDisplayControl.FullPath = selectedPath;
                    if (pathDisplayControl.ToolTip is ToolTip toolTip)
                    {
                        if (toolTip.Content is TextBlock textBlock)
                        {
                            textBlock.Text = selectedPath;
                        }
                    }

                    Properties.Settings.Default.WallpaperSavedPath = selectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// SAVE BUTTON
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // UNDONE: Get and save page data

            TaskbarIcon _TaskBarIcon = (TaskbarIcon)FindResource("NotifyIcon");
            _TaskBarIcon.ShowBalloonTip("INFO", "Saved Settings", BalloonIcon.Info);
        }

        /// <summary>
        /// CANCEL BUTTON
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

    }
}
