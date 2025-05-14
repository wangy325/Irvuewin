using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows;
using System.Windows.Forms;

namespace Irvue_win.src.notify
{
    public static class NotifyClicks
    {
        internal static int currentInterval = 30;

        internal static void ChangeCurrentWallpaperMenuItem_Click(object? sender, EventArgs e)
        {
            // TODO: 切换壁纸
            string imageUrl = "https://hbimg.huaban.com/beeedb5ac346014d36570c37b504e9bc58f980f94d722b-3cEvg7";

            WallpaperUtil wpu = new();
            wpu.SetWallpaper(imageUrl, FetchMode.Random, OS.Windows);
        }

        internal static void LoadPreviousWallpaperMenuItem_Click(object? sender, EventArgs e)
        {
            // TODO: 加载上一个壁纸
            System.Windows.MessageBox.Show("To be implemented...");
        }

        internal static void IntervalMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is int minutes)
            {
                if (menuItem.OwnerItem is ToolStripMenuItem parent)
                {
                    foreach (ToolStripMenuItem item in parent.DropDownItems)
                    {
                        item.Checked = false;
                    }
                }
                // 选中当前项
                menuItem.Checked = true;
                currentInterval = minutes;
                // TODO: 设置壁纸更换间隔

            }
        }

        internal static void SettingsMenuItem_Click(object? sender, EventArgs e)
        {
            // 创建设置窗口的实例
            SettingsWindow settingsWindow = new();

            // 以模态方式显示设置窗口
            // ShowDialog() 会阻塞主窗口，直到设置窗口关闭
            settingsWindow.ShowDialog();

            // 当设置窗口关闭后，代码会继续执行到这里
            // TODO: 在用户点击“保存”后，这里可能需要处理设置窗口返回的结果
        }

        internal static void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        internal static void NotifyIcon_MouseLeftClick(object? sender, EventArgs e)
        {
            if (e is System.Windows.Forms.MouseEventArgs mouseEventArgs && mouseEventArgs.Button == MouseButtons.Left)
            {
                var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                }
            }
        }

    }
}
