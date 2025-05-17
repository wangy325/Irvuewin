using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Irvue_win.src.notify;
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
            NotifyClicks.Settings(sender, args);
        }

        private void Exit_Click(Object sender, RoutedEventArgs args)
        {
            _Is_Exit = true;
            _taskbarIcon?.Dispose();
            NotifyClicks.Exit(sender, args);
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
            NotifyClicks.ChangeCurrentWallpaper(sender, args);
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
