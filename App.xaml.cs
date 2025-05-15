using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Irvue_win.src.notify;

namespace Irvue_win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        private TaskbarIcon? _taskbarIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _taskbarIcon = (TaskbarIcon)FindResource("NotifyIcon");

        }

        private void NotifyIcon_TrayMouseClick(Object sender, RoutedEventArgs args)
        {
            NotifyClicks.NotifyIconClick(sender, args);
        }

        private void Settings_Click(Object sender, RoutedEventArgs args)
        {
            NotifyClicks.Settings(sender, args);
        }

        private void Exit_Click(Object sender, RoutedEventArgs args)
        {
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
