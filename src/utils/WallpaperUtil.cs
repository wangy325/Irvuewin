using System.Runtime.InteropServices;
using System.IO;
using System.Net.Http;

namespace Irvuewin.utils
{
    public sealed class WallpaperUtil
    {

        // 声明需要导入的 Windows API 函数 (来自 user32.dll)
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        // 定义用于 SystemParametersInfo 的常量
        private const uint SPI_SETDESKWALLPAPER = 0x14; // 20 (设置桌面壁纸)
        private const uint SPIF_UPDATEINIFILE = 0x01;   // 将修改写入用户配置文件 WIN.INI (虽然现代Windows不完全依赖它,但习惯上设置)
        private const uint SPIF_SENDCHANGE = 0x02;      // 广播 WM_SETTINGCHANGE 消息通知其他应用壁纸已更改
        private const uint SPIF_SENDWININICHANGE = 0x02; // 与 SPIF_SENDCHANGE 相同，旧名


        public async void SetWallpaper(string channel, Enum mode, Enum os)
        {

            switch (os)
            {
                case OS.Windows:
                    // 组合标志位，表示设置壁纸，并更新配置和通知其他应用
                    uint flags = SPIF_UPDATEINIFILE | SPIF_SENDCHANGE;
                    string? imagePath = await GetWallpaper(channel, mode);
                    // 调用 Windows API 函数
                    // 参数1: 要执行的动作 (设置壁纸)
                    // 参数2: 附加参数 (通常为0，除非有特定需求)
                    // 参数3: 指向壁纸文件路径的字符串
                    // 参数4: 行为标志 (更新配置和通知)
                    if (imagePath != null)
                    {
                        _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, flags);
                    }
                    break;
                case OS.MacOS:
                    // MacOS 设置壁纸的代码
                    // TODO:
                    break;
                case OS.Linux:
                    // Linux 设置壁纸的代码
                    // TODO
                    break;
                default:
                    throw new NotSupportedException("Unsupported OS");
            }
        }

        /// <summary>
        /// Get wallpaper from unsplash or cache
        /// </summary>
        /// <param name="channel"> unsplash wallpaper channel</param>
        /// <param name="mode">load previous or next wallpaper</param>
        /// <returns>file path if succeed, or null</returns>
        private async Task<string?> GetWallpaper(string channel, Enum mode)
        {

            if (mode.Equals(FetchMode.Previous))
            {
                // TODO: Fetch wallpaper from cache
                return null;
            }
            else
            {
                // TODO: Fetch wall paper from unsplash
                // 创建一个零字节的临时文件
                string tempFilePath = Path.GetTempFileName();
                // 获取 URL 的文件扩展名 (.jpg, .png 等)
                string fileExtension = Path.GetExtension(channel);
                // 将临时文件扩展名改为下载文件的扩展名
                string localImagePath = Path.ChangeExtension(tempFilePath, fileExtension);

                using HttpClient httpClient = new();
                try
                {
                    using (Stream imageStream = await httpClient.GetStreamAsync(channel))
                    {
                        using FileStream fileStream = File.Create(localImagePath);
                        await imageStream.CopyToAsync(fileStream);
                    }
                    Console.WriteLine($"image downloaded：\n{localImagePath}", "成功");
                    return localImagePath;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"download error: {ex.Message}");
                    CleanupDownloadedFile(tempFilePath, localImagePath); // 清理临时文件
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"下载或保存图片时发生错误: {ex.Message}", "错误");
                    CleanupDownloadedFile(tempFilePath, localImagePath); // 清理临时文件
                    return null;
                }
            }
        }

        private void CleanupDownloadedFile(string tempFilePath, string finalFilePath)
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

    public enum OS
    {
        Windows,
        MacOS,
        Linux
    }

    /// <summary>
    /// How to fetch wallpaper
    /// </summary>
    public enum FetchMode
    {
        Previous,
        Random
    }
}
