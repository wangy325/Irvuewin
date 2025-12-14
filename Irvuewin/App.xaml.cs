using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AutoMapper;
using H.NotifyIcon;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;
using System.Windows.Interop;
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

            // Create ChannelsViewModel singleton instance
            // TODO 优化初始化过程
            _channelsViewModel = await ChannelsViewModel.GetInstanceAsync();
            Resources.Add("ChannelsViewModel", _channelsViewModel);

            //  Create a copy of Channels in TrayViewModel
            var trayViewModel = Current.Resources["TrayViewModel"] as TrayViewModel;
            trayViewModel!.AddedChannels = _channelsViewModel.Channels;

            // Load wallpaper sequence cache
            var randomWallpaper = Irvuewin.Properties.Settings.Default.RandomWallpaper;
            Console.WriteLine($@"RandomWallpaper: {randomWallpaper}");
            if (!randomWallpaper)
            {
                await TrayMenuHelper.LoadCachedSequence();
            }

            // Async Change wallpaper when app start
            // TrayMenuHelper.CheckPointer();
            // _ = TrayMenuHelper.ChangeAllWallpaper().ConfigureAwait(false);

            // Init wallpaper change schedule Timer
            TrayMenuHelper.InitWallpaperChangeScheduler();

            if (FindResource("NotifyIcon") is TaskbarIcon taskbarIcon)
            {
                _taskbarIcon = taskbarIcon;
                _taskbarIcon.ForceCreate();
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (_isExit) return;
            _taskbarIcon?.Dispose();
            Current.Shutdown();
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
            if (string.IsNullOrWhiteSpace(dest))
            {
                dest = IAppConst.DefaultWallpaperDownloadDir;
            }

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

        private Views.DpiAnchorWindow? _anchorWindow;

        /// <summary>
        /// 在菜单栏弹出时获取显示器的DPI信息，用于解决多显示器DPI不一致的显示问题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrayMouseRight_Click(object sender, RoutedEventArgs e)
        {
            // Create anchor window if needed
            if (_anchorWindow == null)
            {
                _anchorWindow = new Views.DpiAnchorWindow();
                // Ensure handle is created
                _anchorWindow.Show();
                _anchorWindow.Hide();
            }

            // Get mouse position (Physical pixels)
            var point = System.Windows.Forms.Cursor.Position;

            // Use SetWindowPos to position the anchor window at the exact physical coordinates
            var helper = new WindowInteropHelper(_anchorWindow);
            NativeMethods.SetWindowPos(helper.Handle, NativeMethods.HWND_TOP, point.X, point.Y, 0, 0, 
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);

            // Set foreground window to ensure menu closes on outside click
            NativeMethods.SetForegroundWindow(helper.Handle);
            
            // Get Context Menu from Resources
            if (FindResource("TrayContextMenu") is not ContextMenu contextMenu) return;

            contextMenu.PlacementTarget = _anchorWindow;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom; 
            contextMenu.IsOpen = true;
            
            contextMenu.Closed += (s, args) => _anchorWindow.Hide();
        }
    }
}