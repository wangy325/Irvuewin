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
        private TaskbarIcon _taskbarIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _taskbarIcon = (TaskbarIcon)FindResource("NotifyIcon");

        }

        private void NotifyIcon_TrayMouseClick(Object sender, RoutedEventArgs args)
        {
            NotifyClicks.NotifyIcon_MouseLeftClick(sender, args);
        }

        private void SettingsMenuItem_Click(Object sender, RoutedEventArgs args)
        {
            NotifyClicks.SettingsMenuItem_Click(sender, args);
        }

        private void ChangeWallpaperNowMenuItem_Click(Object sender, RoutedEventArgs args)
        {
            NotifyClicks.ChangeCurrentWallpaperMenuItem_Click(sender, args);
        }

        private void ExitMenuItem_Click(Object sender, RoutedEventArgs args)
        {
            NotifyClicks.ExitMenuItem_Click(sender, args);
        }

        private void LoadPreviousWallpaper_Click(object sender, RoutedEventArgs e)
        {

        }

        private void WallpaperChangeInterval_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
