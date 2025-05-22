using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Irvue_win.src.utils;
using Irvue_win.Properties;
using Irvue_win.src.models;

namespace Irvue_win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        private TaskbarIcon? _TaskbarIcon;
        private bool _Is_Exit;
        private readonly WallpaperUtil _WallpaperUtil = new();


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _TaskbarIcon = (TaskbarIcon)FindResource("NotifyIcon");

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (!_Is_Exit)
            { 
                _TaskbarIcon?.Dispose();
                Application.Current.Shutdown();
            }
        }

        private void Settings_Click(Object sender, RoutedEventArgs args)
        {
            // 创建设置窗口的实例
            SettingsWindow settingsWindow = new();

            // 
            //settingsWindow.Owner = this;

            // 以模态方式显示设置窗口
            // ShowDialog() 会阻塞主窗口，直到设置窗口关闭
            settingsWindow.ShowDialog();

            // 当设置窗口关闭后，代码会继续执行到这里
            // TODO: 在用户点击“保存”后，这里可能需要处理设置窗口返回的结果
        }

        private void Exit_Click(Object sender, RoutedEventArgs args)
        {
            _Is_Exit = true;
            _TaskbarIcon?.Dispose();
            Application.Current.Shutdown();
        }

        private void LoadPreviousWallpaper_Click(object sender, RoutedEventArgs e)
        {

        }

        private void WallpaperChangeInterval_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem clickedItem)
            {
                var parent = ItemsControl.ItemsControlFromItemContainer(clickedItem);
                if (parent != null)
                {
                    foreach (var item in parent.Items)
                    {
                        if (parent.ItemContainerGenerator.ContainerFromItem(item) is MenuItem menuItem && menuItem != clickedItem)
                        {
                            menuItem.IsChecked = false;
                        }
                    }
                    clickedItem.IsChecked = true;
                }
            }
            // save configure
            Irvue_win.Properties.Settings.Default.Save();
            System.Diagnostics.Debug.WriteLine($"Current Interval: {Irvue_win.Properties.Settings.Default.WallpaperChangeInterval} ");
        }

        private void ChangeCurrentWallpaper_Click(Object sender, RoutedEventArgs args)
        {
            // TODO: 切换壁纸
            //string imageUrl = "https://hbimg.huaban.com/beeedb5ac346014d36570c37b504e9bc58f980f94d722b-3cEvg7";
            string imageUrl = "https://gd-hbimg.huaban.com/4d7cf515c2bb64e3c01fd1296051d73b4af17383373d09-Xq5iSg";

            _WallpaperUtil.SetWallpaper(imageUrl, FetchMode.Random, OS.Windows);
        }

        private void DownloadCurrentWallpaper_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeAllWallpaper_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AboutCurrentWallpaper_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnsplashChannel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnsplashChannelManage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChannelRadio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem clickedItem)
            {
                var parent = ItemsControl.ItemsControlFromItemContainer(clickedItem);
                if (parent != null)
                {
                    foreach (var item in parent.Items)
                    {
                        if (parent.ItemContainerGenerator.ContainerFromItem(item) is MenuItem menuItem && menuItem != clickedItem)
                        {
                            menuItem.IsChecked = false;
                        }
                    }
                    clickedItem.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// 试图在菜单栏弹出后单击鼠标左键（不选中任何内容）时隐藏菜单栏
        /// 貌似是冗余
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrayMouseLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_TaskbarIcon != null && _TaskbarIcon.IsVisible)
            {
                _TaskbarIcon.ContextMenu.IsOpen = false;
            }
        }

        private void RandomSwitch_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"RandomSwitch: {Settings.Default.RandomWallpaper}");
            Settings.Default.Save();
        }
    }

}
