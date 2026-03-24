using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Irvuewin.Models.Unsplash;
using Microsoft.Win32;
using Serilog;

namespace Irvuewin.Helpers.Utils
{
    // Windows 8 or later required
    [ComImport]
    [Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDesktopWallpaper
    {
        void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorId,
            [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorId);

        void GetMonitorDevicePathAt(uint monitorIndex,
            [MarshalAs(UnmanagedType.LPWStr)] out string monitorId);

        uint GetMonitorDevicePathCount();

        RECT GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorId);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }


    [ComImport]
    [Guid("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD")]
    internal class DesktopWallpaperClass
    {
    }

    public static class WallpaperUtil
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(WallpaperUtil));

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
        // private static extern bool SystemParametersInfo(uint uAction, uint uParam, IntPtr lpvParam, uint fuWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;
        private const uint SPIF_UPDATEINIFILE = 0x01;

        private const uint SPIF_SENDCHANGE = 0x02;

        // 组合标志位，表示设置壁纸，并更新配置和通知其他应用
        private const uint Flags = SPIF_UPDATEINIFILE | SPIF_SENDCHANGE;

        // private static readonly IDesktopWallpaper DesktopWallpaper = (IDesktopWallpaper)new DesktopWallpaperClass();

        /// <summary>
        /// Setup wallpaper(s) for display(s).
        /// </summary>
        /// <param name="photos">Photo(s) to set up as wallpaper</param>
        /// <param name="imagePath">Photos' local disk path, default null</param>
        /// <returns><see cref="WallpaperSetUpResult"/>. null if exception occurs.</returns>
        /// <remarks>
        /// <para>If photos count = 1, then return <see cref="WallpaperSetUpResult.UnifiedWallpaperPath"/></para>
        /// <para>Else return <see cref="WallpaperSetUpResult.PerDisplayWallpapers"/></para>
        /// </remarks>
        public static async Task<WallpaperSetUpResult?> SetWallpaper(List<UnsplashPhoto> photos,
            string? imagePath = null)
        {
            IDesktopWallpaper? desktopWallpaper = null;
            try
            {
                desktopWallpaper = (IDesktopWallpaper)new DesktopWallpaperClass();

                SetWallpaperMode();
                const string deviceNamePrefix = "\\\\.\\DISPLAY";
                var kvp = new Dictionary<string, string>();
                var monitorCount = desktopWallpaper.GetMonitorDevicePathCount();
                var path = await GetWallpaperFullPath(photos[0], imagePath);
                if (photos.Count == 1)
                {
                    for (uint i = 0; i < monitorCount; i++)
                    {
                        desktopWallpaper.GetMonitorDevicePathAt(i, out var monitorId);
                        desktopWallpaper.SetWallpaper(monitorId, path);
                    }

                    return new WallpaperSetUpResult { UnifiedWallpaperPath = path };
                }

                for (uint i = 0; i < monitorCount;)
                {
                    path = await GetWallpaperFullPath(photos[(int)i], imagePath);
                    desktopWallpaper.GetMonitorDevicePathAt(i, out var monitorId);
                    desktopWallpaper.SetWallpaper(monitorId, path);
                    kvp[deviceNamePrefix + ++i] = path;
                }

                return new WallpaperSetUpResult { PerDisplayWallpapers = kvp };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, @"Setting wallpaper error: {ExMessage}", ex.Message);
                return null;
            }
            finally
            {
                if (desktopWallpaper != null) Marshal.ReleaseComObject(desktopWallpaper);
            }
        }


        /// <summary>
        /// Set wallpaper for specify display. 
        /// </summary>
        /// <param name="display"><see cref="Display"/></param>
        /// <param name="photo"><see cref="UnsplashPhoto"/></param>
        /// <param name="imagePath">nullable, photo full disk path</param>
        /// <returns>null if fails, else wallpaper path.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public static async Task<string?> SetWallpaperForSpecificMonitor(Display display, UnsplashPhoto? photo,
            string? imagePath = null)
        {
            IDesktopWallpaper? desktopWallpaper = null;
            try
            {
                desktopWallpaper = (IDesktopWallpaper)new DesktopWallpaperClass();

                SetWallpaperMode();

                var path = await GetWallpaperFullPath(photo, imagePath);

                var monitorId = GetMonitorIdFromDisplay(desktopWallpaper, display);
                if (monitorId == null) throw new NullReferenceException("MonitorId can not be null.");
                desktopWallpaper.SetWallpaper(monitorId, path);
                Logger.Information(@"Set wallpaper for {0}", display.Name);
                return path;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, @"Setting wallpaper error: {0}", ex.Message);
                return null;
            }
            finally
            {
                if (desktopWallpaper != null) Marshal.ReleaseComObject(desktopWallpaper);
            }
        }

        // set all displays' wallpaper by link/local disk path
        [Obsolete("Use SetWallpaper or SetWallpaperForSpecificMonitor(Display) instead") ]
        public static async Task<string?> SetWallpaperLegacy(UnsplashPhoto? photo, string? path = null)
        {
            SetWallpaperMode();
            var imagePath = await GetWallpaperFullPath(photo, path);
            _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, Flags);
            return imagePath;
        }

        private static void SetWallpaperMode()
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop", true);
            switch (Properties.Settings.Default.WallpaperMode)
            {
                case 0:
                    // fill
                    key.SetValue(@"WallpaperStyle", "10");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case 1:
                    // fit
                    key.SetValue(@"WallpaperStyle", "6");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case 2:
                    // center
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case 3:
                    // stretch
                    key.SetValue(@"WallpaperStyle", "2");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case 4:
                    // tile 平铺
                    key.SetValue(@"WallpaperStyle", "1");
                    key.SetValue(@"TileWallpaper", "1");
                    break;
                default:
                    key.SetValue(@"WallpaperStyle", "10");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
            }
        }

        private static string? GetMonitorIdFromDisplay(IDesktopWallpaper desktopWallpaper, Display display)
        {
            var count = desktopWallpaper.GetMonitorDevicePathCount();

            for (uint i = 0; i < count; i++)
            {
                desktopWallpaper.GetMonitorDevicePathAt(i, out var monitorId);
                var rect = desktopWallpaper.GetMonitorRECT(monitorId);

                // position matcher
                if (rect.left == display.Left && rect.top == display.Top)
                    return monitorId;
            }

            return null;
        }

        /// <summary>
        /// Get wallpaper's full disk path.
        /// </summary>
        /// <param name="photo">UnsplashPhoto</param>
        /// <param name="imagePath">wallpaper local full disk path, nullable</param>
        /// <returns>Wallpaper's file path if successes, or throw ex</returns>
        public static async Task<string> GetWallpaperFullPath(UnsplashPhoto? photo, string? imagePath)
        {
            string path;
            if (imagePath != null)
            {
                path = imagePath;
            }
            else
            {
                ArgumentNullException.ThrowIfNull(photo);
                // path = await GetWallpaperPath(photo);
                const string fileExtension = ".jpg";
                // wallpaper tmp folder
                var dir = FileUtils.CachedWallpaperFolder;
                // image name
                var imageName = photo.Id + fileExtension;
                var localImagePath = Path.Combine(dir, imageName);
                if (File.Exists(localImagePath)) return localImagePath;
                // Fetch from web
                using HttpClient httpClient = new();
                var uriString = photo.Urls.Raw?.ToString();
                // Use cloudflare proxy to avoid 443 errors
                uriString = uriString?.Replace(IAppConst.OriginImageUrl, IAppConst.ImageProxyUrl);

                try
                {
                    await using var imageStream = await httpClient.GetStreamAsync(uriString);
                    await using var fileStream = File.Create(localImagePath);
                    await imageStream.CopyToAsync(fileStream);
                    path = localImagePath;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, @"Load wallpaper path error: {ExMessage}", ex.Message);
                    throw new Exception("Load wallpaper path error");
                }
            }

            return path;
        }

        public static string[] GetAllWallpapers()
        {
            IDesktopWallpaper? desktopWallpaper = null;
            try
            {
                desktopWallpaper = (IDesktopWallpaper)new DesktopWallpaperClass();
                var monitorCount = desktopWallpaper.GetMonitorDevicePathCount();

                var wallpapers = new string[monitorCount];
                for (uint i = 0; i < monitorCount; i++)
                {
                    desktopWallpaper.GetMonitorDevicePathAt(i, out var monitorId);

                    var wallpaperPath = desktopWallpaper.GetWallpaper(monitorId);
                    wallpapers[i] = wallpaperPath;
                }

                return wallpapers;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, @"Get wallpaper path error: {ExMessage}", ex.Message);
                return [];
            }
            finally
            {
                if (desktopWallpaper != null) Marshal.ReleaseComObject(desktopWallpaper);
            }
        }

        public class WallpaperSetUpResult
        {
            /// <summary>
            /// Wallpaper full disk path.
            /// </summary>
            public string? UnifiedWallpaperPath { get; init; }

            /// <summary>
            /// <para>Key: Display name</para>
            /// <para>Value: Wallpaper full disk path</para>
            /// </summary>
            public Dictionary<string, string>? PerDisplayWallpapers { get; init; }

            public bool IsUnified => UnifiedWallpaperPath != null;
        }
    }
}