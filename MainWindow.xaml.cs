using System.Windows;
using Irvue_win.src.utils;

namespace Irvue_win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetWallPaperButton_Click(object? sender, RoutedEventArgs? e)
        {

            string imageUrl = "https://gd-hbimg.huaban.com/4d7cf515c2bb64e3c01fd1296051d73b4af17383373d09-Xq5iSg";

            WallpaperUtil wpu = new();
            wpu.SetWallpaper(imageUrl, FetchMode.Random, OS.Windows);
        }


        // 通知栏（状态栏图标） #HardCodet包

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 取消关闭窗口事件
            e.Cancel = true;
            this.Hide(); // 隐藏窗口而不是关闭
            //notifyIcon.ShowBalloonTip(1000, "IrvueWin", "IrvueWin is running in the background.", ToolTipIcon.Info);
        }

    }
}