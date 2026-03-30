using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using AutoMapper;
using H.NotifyIcon;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;
using System.Windows.Interop;
using System.Windows.Media;
using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.HTTP;
using Irvuewin.Helpers.Logging;
using Irvuewin.Views;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Localization = Irvuewin.Helpers.Localization;

namespace Irvuewin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App
    {
        private static readonly ILogger Logger = Log.ForContext<App>();
        private TaskbarIcon? _taskbarIcon;
        private bool _isExit;

        // private ChannelsViewModel? _channelsViewModel;

        // Used for tray menu position
        private DpiAnchorWindow? _anchorWindow;
        private ContextMenu? _trayContextMenu;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (Irvuewin.Properties.Settings.Default.UpgradeRequired)
            {
                Irvuewin.Properties.Settings.Default.Upgrade();
                Irvuewin.Properties.Settings.Default.UpgradeRequired = false;
                Irvuewin.Properties.Settings.Default.Save();
            }

            // Apply Language Setting
            var language = Irvuewin.Properties.Settings.Default.Language;
            Localization.Instance.SetCulture(language);

            // Apply Theme Setting
            var theme = Irvuewin.Properties.Settings.Default.Theme;
            ThemeManager.SetTheme((ThemeType)theme);

            // Init Logger
            LogHelper.Init();
            Logger.Information("Application Starting...");

            SetupExceptionHandling();

            // Init _anchorWindow
            {
                _anchorWindow = new DpiAnchorWindow();
                // Ensure handle is created
                _anchorWindow.Show();
                _anchorWindow.Hide();
            }

            // AutoMapper config
            var mapperCfg = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<UnsplashChannel, ChannelViewModel>();
                }, new NullLoggerFactory());
            var mapper = mapperCfg.CreateMapper();
            MapperProvider.Mapper = mapper;

            // Initialize ChannelsViewModel asynchronously to avoid blocking UI thread
            _ = Task.Run(async () =>
            {
                var vm = await ChannelsViewModel.GetInstanceAsync();
                // _channelsViewModel = vm;
                
                await Dispatcher.InvokeAsync(() =>
                {
                    if (Resources.Contains("ChannelsViewModel")) Resources["ChannelsViewModel"] = vm;
                    else Resources.Add("ChannelsViewModel", vm);

                    // Sync to TrayViewModel
                    if (Current.Resources["TrayViewModel"] is TrayViewModel trayViewModel)
                    {
                        trayViewModel.AddedChannels = vm.Channels;
                    }

                    // Pre-create Channels window hidden to warm up UI
                    _ = WindowManager.PreloadWindow(nameof(Channels), () => new Channels());
                }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            });
            
            // Init Wallpaper pool and Sync Worker
            WallpaperPoolManager.Initialize(IHttpClient.GetUnsplashHttpService());
            UnsplashSyncWorker.Initialize(IHttpClient.GetUnsplashHttpService());

            // Load wallpaper sequence cache
            var randomWallpaper = Irvuewin.Properties.Settings.Default.RandomWallpaper;
            Logger.Information("RandomWallpaper: {RandomWallpaper}", randomWallpaper);

            // Init wallpaper change schedule Timer
            IrvuewinCore.InitWallpaperChangeScheduler();

            if (FindResource("NotifyIcon") is TaskbarIcon taskbarIcon)
            {
                _taskbarIcon = taskbarIcon;
                _taskbarIcon.ForceCreate();
            }

            // Enhanced preloading of ContextMenu
            Dispatcher.InvokeAsync(() =>
            {
                if (FindResource("TrayContextMenu") is not ContextMenu contextMenu) return;
                _trayContextMenu = contextMenu;
                
                // Force layout pass to warm up bindings and templates
                _trayContextMenu.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                _trayContextMenu.ApplyTemplate();
                
                _trayContextMenu.Closed += (_, _) => _anchorWindow?.Hide();
                
                // Also preload the About submenu if possible
                if (_trayContextMenu.Items.OfType<MenuItem>().FirstOrDefault(m => m.Name == "AboutCurrentWallpaper") is { } aboutMenu)
                {
                    aboutMenu.ApplyTemplate();
                }
            }, System.Windows.Threading.DispatcherPriority.Background);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (_isExit) return;
            _taskbarIcon?.Dispose();
            Logger.Information("Application Exiting...");
            LogHelper.CloseAndFlush();
            Current.Shutdown();
        }

        private void SetupExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                Logger.Fatal(exception, "AppDomain.CurrentDomain.UnhandledException");
                LogHelper.CloseAndFlush();
            };

            // Catch exceptions from the main UI dispatcher thread.
            DispatcherUnhandledException += (_, args) =>
            {
                Logger.Fatal(args.Exception, "DispatcherUnhandledException");
                // args.Handled = true; // Uncomment if we want to prevent crash, but hazardous.
                LogHelper.CloseAndFlush();
            };

            // Catch exceptions from unobserved tasks.
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                Logger.Error(args.Exception, "TaskScheduler.UnobservedTaskException");
                args.SetObserved();
            };
        }

        public void RefreshTrayIcon()
        {
            if (FindResource("NotifyIcon") is not TaskbarIcon taskbarIcon) return;

            var geometry = FindResource("Icon_AppLogo") as Geometry;
            var brush = FindResource("PrimaryTextBrush") as Brush;

            if (geometry == null || brush == null) return;
            // Use 32 for better quality on High DPI, but restrict content to 28 for visual padding
            var icon = IconHelper.GenerateIcon(geometry, brush, 32, 28);
            taskbarIcon.Icon = icon;
        }

        //----------------------------------- TrayMenu ---------------------------------//

        private void ChangeCurrentWallpaper_Click(object sender, RoutedEventArgs args)
        {
            IrvuewinCore.ChangeCurrentWallpaper();
        }

        private void ChangeAllWallpaper_Click(object sender, RoutedEventArgs e)
        {
            IrvuewinCore.ChangeAllWallpaper();
        }

        private void LoadPreviousWallpaper_Click(object sender, RoutedEventArgs e)
        {
            IrvuewinCore.PreviousWallpaper();
        }

        private void DownloadCurrentWallpaper_Click(object sender, RoutedEventArgs e)
        {
            var dest = Irvuewin.Properties.Settings.Default.WallpaperSavedPath;
            if (string.IsNullOrWhiteSpace(dest))
            {
                dest = IAppConst.DefaultWallpaperDownloadDir;
            }

            if (!IrvuewinCore.DownloadCurrentWallpaper(dest)) return;
            var openFolder = Irvuewin.Properties.Settings.Default.OpenSavedWallpaper;
            if (openFolder)
            {
                Process.Start("explorer.exe", dest);
            }
        }

        private void LikeCurrentWallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (!FastCacheManager.TryGet(IrvuewinCore.CurrentPointerDisplay.Name, out string? photoId) ||
                photoId is null) return;
            DataBaseService.UpdatePhotoLikedStatus(photoId, true);
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
            IrvuewinCore.UpdateWallpaperChangeScheduler();
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
            // if (!Irvuewin.Properties.Settings.Default.RandomWallpaper)
            // {
            //     IrvuewinCore.SaveCachedSequence();
            // }

            _taskbarIcon?.Dispose();
            Current.Shutdown();
        }


        /// <summary>
        /// 在菜单栏弹出时获取显示器的DPI信息，用于解决多显示器DPI不一致的显示问题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrayMouseRight_Click(object sender, RoutedEventArgs e)
        {
            // Create anchor window if needed
            
            try
            {
                // Get mouse position (Physical pixels)
                var point = Cursor.Position;

                // Use SetWindowPos to position the anchor window at the exact physical coordinates
                var helper = new WindowInteropHelper(_anchorWindow!);
                NativeMethods.SetWindowPos(helper.Handle, NativeMethods.HWND_TOP, point.X, point.Y, 0, 0,
                    NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);

                // Set foreground window to ensure menu closes on outside click
                NativeMethods.SetForegroundWindow(helper.Handle);

                // Get Context Menu from Resources
                if (_trayContextMenu == null)
                {
                    _trayContextMenu = FindResource("TrayContextMenu") as ContextMenu;
                    if (_trayContextMenu != null)
                    {
                        _trayContextMenu.Closed += (_, _) => _anchorWindow?.Hide();
                    }
                }

                if (_trayContextMenu == null) return;

                // Update LikeCurrentWallpaper state
                if (_trayContextMenu.Items.OfType<MenuItem>().FirstOrDefault(m => m.Name == "LikeCurrentWallpaper") is { } likeMenuItem)
                {
                    IrvuewinCore.CheckPointer();
                    if (FastCacheManager.TryGet(IrvuewinCore.CurrentPointerDisplay.Name, out string? photoId) && photoId is not null)
                    {
                        likeMenuItem.IsEnabled = !DataBaseService.IsPhotoLiked(photoId);
                    }
                    else
                    {
                        likeMenuItem.IsEnabled = false;
                    }
                }

                _trayContextMenu.PlacementTarget = _anchorWindow;
                _trayContextMenu.Placement = PlacementMode.Bottom;
                _trayContextMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                // ignored
                Logger.Error("Fatal error: {0}", ex.Message);
            }
        }
    }
}