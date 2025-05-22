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

        private WeakReference<SettingsWindow?> _SettingsWindowRef = new(null);



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
            if (_SettingsWindowRef.TryGetTarget(out var window)
                && window.IsLoaded)
            {
                window.Activate();
                return;
            }

            var newWindow = new SettingsWindow();
            _SettingsWindowRef.SetTarget(newWindow);

            // 窗口关闭事件
            newWindow.Closed += (s, e) => _SettingsWindowRef.SetTarget(null);
            newWindow.Show();
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

        // 打开频道管理页
        private void UnsplashChannelManage_Click(object sender, RoutedEventArgs e)
        {
            Window window = new ChannelsWindow();
            window.Show();
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
