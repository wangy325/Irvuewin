using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Irvuewin.Models.Unsplash;
using Microsoft.Win32;

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
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
        // private static extern bool SystemParametersInfo(uint uAction, uint uParam, IntPtr lpvParam, uint fuWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;
        private const uint SPIF_UPDATEINIFILE = 0x01;

        private const uint SPIF_SENDCHANGE = 0x02;

        // 组合标志位，表示设置壁纸，并更新配置和通知其他应用
        private const uint Flags = SPIF_UPDATEINIFILE | SPIF_SENDCHANGE;

        private static readonly IDesktopWallpaper DesktopWallpaper = (IDesktopWallpaper)new DesktopWallpaperClass();

        public static async Task<WallpaperSetUpResult?> SetWallpaper(List<UnsplashPhoto> photos, string? imagePath = null)
        {
            try
            {
                SetWallpaperMode();
                const string deviceNamePrefix = "\\\\.\\DISPLAY";
                var kvp = new Dictionary<string, string>();
                var monitorCount = DesktopWallpaper.GetMonitorDevicePathCount();
                var path = await GetWallpaperFullPath(photos[0], imagePath);
                if (photos.Count == 1)
                {
                    for (uint i = 0; i < monitorCount; i++)
                    {
                        DesktopWallpaper.GetMonitorDevicePathAt(i, out var monitorId);
                        DesktopWallpaper.SetWallpaper(monitorId, path);
                    }

                    return new WallpaperSetUpResult { UnifiedWallpaperPath = path };
                }
                else
                {
                    for (uint i = 0; i < monitorCount;)
                    {
                        path = await GetWallpaperFullPath(photos[(int)i], imagePath);
                        DesktopWallpaper.GetMonitorDevicePathAt(i, out var monitorId);
                        DesktopWallpaper.SetWallpaper(monitorId, path);
                        kvp[deviceNamePrefix + ++i] = path;
                    }

                    return new WallpaperSetUpResult { PerDisplayWallpapers = kvp };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($@"Setting wallpaper error: {ex.Message}");
                return null;
            }
        }


        public static async Task<string?> SetWallpaperForSpecificMonitor(Display display, UnsplashPhoto? photo,
            string? imagePath = null)
        {
            try
            {
                SetWallpaperMode();

                var path = await GetWallpaperFullPath(photo, imagePath);

                var monitorId = GetMonitorIdFromDisplay(display);
                if (monitorId == null) throw new ArgumentNullException($"MonitorId can not be null.");
                DesktopWallpaper.SetWallpaper(monitorId, path);

                Console.WriteLine(@$"monitorName: {display.Name}, monitorId: {monitorId}");

                return path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($@"Setting wallpaper error: {ex.Message}");
                return null;
            }
        }

        // set all displays' wallpaper by link/local disk path
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

        private static string? GetMonitorIdFromDisplay(Display display)
        {
            var count = DesktopWallpaper.GetMonitorDevicePathCount();

            for (uint i = 0; i < count; i++)
            {
                DesktopWallpaper.GetMonitorDevicePathAt(i, out var monitorId);
                var rect = DesktopWallpaper.GetMonitorRECT(monitorId);

                // position matcher
                if (rect.left == display.Left && rect.top == display.Top)
                    return monitorId;
            }

            return null;
        }

        /// <summary>
        /// Get wallpaper from unsplash or local disk
        /// </summary>
        /// <param name="photo">UnsplashPhoto</param>
        /// <param name="imagePath">wallpaper local full disk path, nullable</param>
        /// <returns>file path if successes, or throw ex</returns>
        private static async Task<string> GetWallpaperFullPath(UnsplashPhoto? photo, string? imagePath)
        {
            string path;
            if (imagePath != null)
            {
                path = imagePath;
            }
            else
            {
                if (photo == null) throw new ArgumentNullException($"Photo can't be null.");
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
                // TODO Crop photo
                var uri = photo.Urls.Raw;
                try
                {
                    await using var imageStream = await httpClient.GetStreamAsync(uri);
                    await using var fileStream = File.Create(localImagePath);
                    await imageStream.CopyToAsync(fileStream);
                    path = localImagePath;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($@"Load wallpaper path error: {ex.Message}");
                    throw new Exception("Load wallpaper path error");
                }
            }

            return path;
        }

        public static string[] GetAllWallpapers()
        {
            try
            {
                var monitorCount = DesktopWallpaper.GetMonitorDevicePathCount();

                var wallpapers = new string[monitorCount];
                for (uint i = 0; i < monitorCount; i++)
                {
                    DesktopWallpaper.GetMonitorDevicePathAt(i, out var monitorId);

                    var wallpaperPath = DesktopWallpaper.GetWallpaper(monitorId);
                    wallpapers[i] = wallpaperPath;
                }

                return wallpapers;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($@"Get wallpaper path error: {ex.Message}");
                return [];
            }
        }
    }
    
    public class WallpaperSetUpResult
    {
        public string? UnifiedWallpaperPath { get; init; }
        
        public Dictionary<string, string>? PerDisplayWallpapers { get; init; }

        public bool IsUnified => UnifiedWallpaperPath != null;
    }

}