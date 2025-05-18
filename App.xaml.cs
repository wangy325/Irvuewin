using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Irvue_win.src.utils;
using Windows.Devices.PointOfService;

namespace Irvue_win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        private TaskbarIcon? _taskbarIcon;
        private bool _Is_Exit;
        private readonly WallpaperUtil _WallpaperUtil = new();


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _taskbarIcon = (TaskbarIcon)FindResource("NotifyIcon");

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (!_Is_Exit)
            { 
                _taskbarIcon?.Dispose();
                Application.Current.Shutdown();
            }
        }

        private void Settings_Click(Object sender, RoutedEventArgs args)
        {
            // 创建设置窗口的实例
            SettingsWindow settingsWindow = new();

            // 以模态方式显示设置窗口
            // ShowDialog() 会阻塞主窗口，直到设置窗口关闭
            settingsWindow.ShowDialog();

            // 当设置窗口关闭后，代码会继续执行到这里
            // TODO: 在用户点击“保存”后，这里可能需要处理设置窗口返回的结果
        }

        private void Exit_Click(Object sender, RoutedEventArgs args)
        {
            _Is_Exit = true;
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
    }

}
