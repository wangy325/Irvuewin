using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoMapper;
using Hardcodet.Wpf.TaskbarNotification;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models;
using Irvuewin.Models.Unsplash;
using Irvuewin.Properties;
using Irvuewin.ViewModels;
using Irvuewin.Views;

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

            // AutoMapper config
            var config = new MapperConfiguration(cfg =>
                cfg.CreateMap<UnsplashChannel, ChannelViewModel>());
            var mapper = config.CreateMapper();
            MapperProvider.Mapper = mapper;
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
                        if (parent.ItemContainerGenerator.ContainerFromItem(item) is MenuItem menuItem &&
                            menuItem != clickedItem)
                        {
                            menuItem.IsChecked = false;
                        }
                    }
                    clickedItem.IsChecked = true;
                }
            }

            // save configure
            Irvuewin.Properties.Settings.Default.Save();
            System.Diagnostics.Debug.WriteLine(
                $"Current Interval: {Irvuewin.Properties.Settings.Default.WallpaperChangeInterval} ");
        }

        private async void  ChangeCurrentWallpaper_Click(object sender, RoutedEventArgs args)
        {
            // TODO: 切换壁纸
            //频道信息
            var channels = Application.Current.Resources["ChannelsViewModel"] as ChannelsViewModel;
            var selectedChannel = channels?.SelectedChannel!;
            TrayMenuHelper.ChangeCurrentWallpaper(selectedChannel);
            
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
            System.Diagnostics.Debug.WriteLine($"RandomSwitch: {Irvuewin.Properties.Settings.Default.RandomWallpaper}");
            Irvuewin.Properties.Settings.Default.Save();
        }

        private void ChannelSelector_Click(object sender, RoutedEventArgs e)
        {
        }

        private void AddNewChannel_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}