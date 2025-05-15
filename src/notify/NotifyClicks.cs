using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Irvue_win.src.notify
{
    public static class NotifyClicks
    {
        internal static int currentInterval = 30;

        internal static WallpaperUtil wallpaperUtil = new();

        internal static void ChangeCurrentWallpaper(object? sender, EventArgs e)
        {
            // TODO: 切换壁纸
            //string imageUrl = "https://hbimg.huaban.com/beeedb5ac346014d36570c37b504e9bc58f980f94d722b-3cEvg7";
            string imageUrl = "https://gd-hbimg.huaban.com/4d7cf515c2bb64e3c01fd1296051d73b4af17383373d09-Xq5iSg";

            wallpaperUtil.SetWallpaper(imageUrl, FetchMode.Random, OS.Windows);
        }

        internal static void LoadPreviousWallpaper(object? sender, EventArgs e)
        {
            // TODO: 加载上一个壁纸
            MessageBox.Show("To be implemented...");
        }

        internal static void DownloadCurrentWallpaper(Object sender, EventArgs e)
        { 
            // TODO: 不同的桌面加载不同的壁纸
        }


        internal static void Settings(object? sender, EventArgs e)
        {
            // 创建设置窗口的实例
            SettingsWindow settingsWindow = new();

            // 以模态方式显示设置窗口
            // ShowDialog() 会阻塞主窗口，直到设置窗口关闭
            settingsWindow.ShowDialog();

            // 当设置窗口关闭后，代码会继续执行到这里
            // TODO: 在用户点击“保存”后，这里可能需要处理设置窗口返回的结果
        }

        internal static void Exit(object? sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        internal static void NotifyIconClick(object? sender, EventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            }

        }

    }
}
