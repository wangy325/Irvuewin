using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Irvuewin.utils;
using Irvuewin.Properties;
using Irvuewin.models;

namespace Irvuewin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App
    {
        private TaskbarIcon? _taskbarIcon;
        private bool _isExit;
        private readonly WallpaperUtil _wallpaperUtil = new();
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (FindResource("NotifyIcon") is TaskbarIcon taskbarIcon)
                _taskbarIcon = taskbarIcon;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (!_isExit)
            { 
                _taskbarIcon?.Dispose();
                Current.Shutdown();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs args)
        {
            WindowManager.ShowWindow("SettingsWindow", () => new SettingsWindow());
        }

        private void Exit_Click(object sender, RoutedEventArgs args)
        {
            _isExit = true;
            _taskbarIcon?.Dispose();
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
            Settings.Default.Save();
            System.Diagnostics.Debug.WriteLine($"Current Interval: {Settings.Default.WallpaperChangeInterval} ");
        }

        private void ChangeCurrentWallpaper_Click(object sender, RoutedEventArgs args)
        {
            // TODO: 切换壁纸
            //string imageUrl = "https://hbimg.huaban.com/beeedb5ac346014d36570c37b504e9bc58f980f94d722b-3cEvg7";
            string imageUrl = "https://gd-hbimg.huaban.com/4d7cf515c2bb64e3c01fd1296051d73b4af17383373d09-Xq5iSg";

            _wallpaperUtil.SetWallpaper(imageUrl, FetchMode.Random, OS.Windows);
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

        // 打开频道管理页
        private void UnsplashChannelManage_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowWindow("ChannelsWindow", () => new ChannelsWindow());
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
            if (_taskbarIcon is not { IsVisible: true }) return;
            if (_taskbarIcon.ContextMenu != null) _taskbarIcon.ContextMenu.IsOpen = false;
        }

        private void RandomSwitch_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"RandomSwitch: {Settings.Default.RandomWallpaper}");
            Settings.Default.Save();
        }
    }

}
