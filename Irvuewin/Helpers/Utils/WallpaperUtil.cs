using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Irvuewin.Models.Unsplash;
using Microsoft.Win32;

namespace Irvuewin.Helpers.Utils
{
    public static class WallpaperUtil
    {
        // 声明需要导入的 Windows API 函数 (来自 user32.dll)
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        // 定义用于 SystemParametersInfo 的常量
        private const uint SPI_SETDESKWALLPAPER = 0x14; // 20 (设置桌面壁纸)
        private const uint SPIF_UPDATEINIFILE = 0x01; // 将修改写入用户配置文件 WIN.INI (虽然现代Windows不完全依赖它,但习惯上设置)

        private const uint SPIF_SENDCHANGE = 0x02; // 广播 WM_SETTINGCHANGE 消息通知其他应用壁纸已更改

        public static async Task<string?> SetWallpaper(UnsplashPhoto? photo, string? path = null)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop", true)!)
            {
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
                        // stretch
                        key.SetValue(@"WallpaperStyle", "2");
                        key.SetValue(@"TileWallpaper", "0");
                        break;
                    case 3:
                        // center
                        key.SetValue(@"WallpaperStyle", "0");
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

            // 组合标志位，表示设置壁纸，并更新配置和通知其他应用
            const uint flags = SPIF_UPDATEINIFILE | SPIF_SENDCHANGE;
            if (path != null)
            {
                _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, flags);
                return path;
            }

            if (photo == null) throw new ArgumentNullException($"Photo can't be null.");
            // download photo
            var imagePath = await GetWallpaper(photo);
            if (imagePath != null)
            {
                _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, flags);
            }

            return imagePath;
        }

        /// <summary>
        /// Get wallpaper from unsplash or local disk
        /// </summary>
        /// <param name="photo">UnsplashPhoto</param>
        /// <returns>file path if successes, or null</returns>
        private static async Task<string?> GetWallpaper(UnsplashPhoto photo)
        {
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
                return localImagePath;
            }
            catch (Exception ex)
            {
                // CleanupDownloadedFile(tempFilePath, localImagePath);
                return null;
            }
        }


        private static void CleanupDownloadedFile(string tempFilePath, string finalFilePath)
        {
            try
            {
                if (File.Exists(finalFilePath))
                {
                    File.Delete(finalFilePath);
                }

                // 有时候 GetTempFileName 创建的原始文件也可能残留
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                // 清理失败的错误可以忽略或记录，不影响主流程
                System.Diagnostics.Debug.WriteLine($"清理文件时发生错误: {ex.Message}");
            }
        }
    }
}