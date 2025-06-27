using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AutoMapper;
using Hardcodet.Wpf.TaskbarNotification;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
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
        private ChannelsViewModel? _channelsViewModel;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // AutoMapper config
            var config = new MapperConfiguration(cfg =>
                cfg.CreateMap<UnsplashChannel, ChannelViewModel>());
            var mapper = config.CreateMapper();
            MapperProvider.Mapper = mapper;

            /*await Application.Current.Dispatcher.InvokeAsync(() =>
            {
            });*/
            
            // Create ChannelsViewModel singleton instance
            _channelsViewModel = await ChannelsViewModel.GetInstanceAsync();
            Resources.Add("ChannelsViewModel", _channelsViewModel);
            
            //  Create a copy of Channels in TrayViewModel
            TrayViewModel.Channels = _channelsViewModel.Channels;

            // Load wallpaper sequence cache
            // 启动时若未启用随机壁纸则加载序列缓存
            var randomWallpaper = Irvuewin.Properties.Settings.Default.RandomWallpaper;
            Console.WriteLine($@"RandomWallpaper: {randomWallpaper}");
            if (!randomWallpaper)
            {
                await TrayMenuHelper.LoadCachedSequence();
            }

            // Change wallpaper when app start
            await TrayMenuHelper.ChangeCurrentWallpaper();
            
            // Init wallpaper change schedule Timer
            TrayMenuHelper.InitWallpaperChangeScheduler();

            if (FindResource("NotifyIcon") is TaskbarIcon taskbarIcon)
                _taskbarIcon = taskbarIcon;
            base.OnStartup(e);
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            // TODO Is necessary?
            if (!_isExit)
            {
                _taskbarIcon?.Dispose();
                Current.Shutdown();
            }
        }

        //----------------------------------- TrayMenu ---------------------------------//

        private async void ChangeCurrentWallpaper_Click(object sender, RoutedEventArgs args)
        {
            await TrayMenuHelper.ChangeCurrentWallpaper();
        }

        private void ChangeAllWallpaper_Click(object sender, RoutedEventArgs e)
        {
            TrayMenuHelper.ChangeAllWallpaper();
        }

        private void LoadPreviousWallpaper_Click(object sender, RoutedEventArgs e)
        {
            TrayMenuHelper.PreviousWallpaper();
        }

        private void DownloadCurrentWallpaper_Click(object sender, RoutedEventArgs e)
        {
            var dest = Irvuewin.Properties.Settings.Default.WallpaperSavedPath;
            if (!TrayMenuHelper.DownloadCurrentWallpaper(dest)) return;
            var openFolder = Irvuewin.Properties.Settings.Default.OpenSavedWallpaper;
            if (openFolder)
            {
                Process.Start("explorer.exe", dest);
            }
        }

        //----------------------------- Channels ---------------------------//

        // 打开频道管理页
        private void ManageChannel_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowWindow(nameof(Channels), () => new Channels());
        }


        //------------------------------ Wallpapers ---------------------------------//

        private void WallpaperChangeInterval_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem clickedItem) return;
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

            // save configure
            ushort.TryParse(clickedItem.Tag as string, out var result);
            Irvuewin.Properties.Settings.Default.WallpaperChangeInterval = result;
            Irvuewin.Properties.Settings.Default.Save();
            // update Timer
            TrayMenuHelper.UpdateWallpaperChangeScheduler();
        }

        //------------------------------------ Settings --------------------------------//

        private void Settings_Click(object sender, RoutedEventArgs args)
        {
            WindowManager.ShowWindow(nameof(Views.Settings), () => new Views.Settings());
        }

        private void Exit_Click(object sender, RoutedEventArgs args)
        {
            _isExit = true;
            // 退出时若未启用随机壁纸则保存缓存（sequence may be modified）
            var randomWallpaper = Irvuewin.Properties.Settings.Default.RandomWallpaper;
            Console.WriteLine($@"RandomWallpaper: {randomWallpaper}");
            if (!randomWallpaper)
            {
                TrayMenuHelper.SaveCachedSequence();
            }

            _taskbarIcon?.Dispose();
            Current.Shutdown();
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
    }
}