using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// system api
using System.Runtime.InteropServices;
using System.Net.Http; // 用于 HttpClient
using System.IO;       // 用于文件操作 (Stream, File)
using System.Threading.Tasks; // 用于异步操作 (async/await)



namespace Irvue_win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // 声明需要导入的 Windows API 函数 (来自 user32.dll)
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        // 定义用于 SystemParametersInfo 的常量
        private const uint SPI_SETDESKWALLPAPER = 0x14; // 20 (设置桌面壁纸)
        private const uint SPIF_UPDATEINIFILE = 0x01;   // 将修改写入用户配置文件 WIN.INI (虽然现代Windows不完全依赖它,但习惯上设置)
        private const uint SPIF_SENDCHANGE = 0x02;      // 广播 WM_SETTINGCHANGE 消息通知其他应用壁纸已更改
        private const uint SPIF_SENDWININICHANGE = 0x02; // 与 SPIF_SENDCHANGE 相同，旧名

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void setWallPaperButton_Click(object sender, RoutedEventArgs e)
        {

            string imageUrl = "https://www.w3schools.com/w3css/img_lights.jpg";

            string tempFilePath = System.IO.Path.GetTempFileName(); // 创建一个零字节的临时文件
            string fileExtension = System.IO.Path.GetExtension(imageUrl); // 获取 URL 的文件扩展名 (.jpg, .png 等)
            string localImagePath = System.IO.Path.ChangeExtension(tempFilePath, fileExtension); // 将临时文件扩展名改为下载文件的扩展名


            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    using (Stream imageStream = await httpClient.GetStreamAsync(imageUrl))
                    {
                        using (FileStream fileStream = File.Create(localImagePath))
                        {
                            await imageStream.CopyToAsync(fileStream);
                        }
                    }

                    // *** 就是这里！调用设置壁纸的方法，传入下载文件的路径 ***
                    SetDesktopWallpaper(localImagePath);

                    MessageBox.Show($"图片已下载并尝试设置为壁纸：\n{localImagePath}", "成功");
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"download error: {ex.Message}");
                    CleanupDownloadedFile(tempFilePath, localImagePath); // 清理临时文件
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"下载或保存图片时发生错误: {ex.Message}", "错误");
                    CleanupDownloadedFile(tempFilePath, localImagePath); // 清理临时文件
                    return; 
                }
            }
        }


        /// <summary>
        /// 设置 Windows 桌面壁纸
        /// </summary>
        /// <param name="imagePath">壁纸图片文件的完整路径</param>
        public static void SetDesktopWallpaper(string imagePath)
        {
            // 组合标志位，表示设置壁纸，并更新配置和通知其他应用
            uint flags = SPIF_UPDATEINIFILE | SPIF_SENDCHANGE;

            // 调用 Windows API 函数
            // 参数1: 要执行的动作 (设置壁纸)
            // 参数2: 附加参数 (通常为0，除非有特定需求)
            // 参数3: 指向壁纸文件路径的字符串
            // 参数4: 行为标志 (更新配置和通知)
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, flags);
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
}