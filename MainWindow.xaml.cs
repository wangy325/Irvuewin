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
using System.Windows.Forms; // 用于 NotifyIcon
using System.Drawing;       // 用于 Icon
using System.Threading;     // 可能需要，但这里主要用在 Dispose
using System.ComponentModel; // 用于 CancelEventArgs

using System.Threading.Tasks; // 用于异步操作 (async/await)

using Irvue_win.src;
using Irvue_win.src.notify;

namespace Irvue_win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // system tray icon 
        private NotifyIcon? notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            SetNotifyIcon();
        }

        private void SetWallPaperButton_Click(object? sender, RoutedEventArgs? e)
        {

            string imageUrl = "https://www.w3schools.com/w3css/img_lights.jpg";

            WallpaperUtil wpu = new();
            wpu.SetWallpaper(imageUrl, FetchMode.Random, OS.Windows);
        }

        // 通知栏（状态栏图标）
        private void SetNotifyIcon()
        {
            notifyIcon = new NotifyIcon();

            // TODO 下次更新壁纸的时间

            // icons
            notifyIcon.Icon = new Icon("icons\\tray.ico");

            // mouse hover prompt
            notifyIcon.Text = "IrvueWin";
            ContextMenuStrip contextMenu = new();

            WallpaperGroupMenuItems(contextMenu);


            ChannelGroupMenuItems(contextMenu);


            SettingsAndExitMenuItems(contextMenu);
            

            notifyIcon.ContextMenuStrip = contextMenu;

            SetMenuItemPadding(contextMenu.Items, new Padding(4, 8, 4, 8));

            // handle mouse click events
            notifyIcon.Click += NotifyClicks.NotifyIcon_MouseLeftClick;

            notifyIcon.Visible = true;
            System.Windows.Application.Current.Exit += Application_Exit;
        }

        private void SettingsAndExitMenuItems(ContextMenuStrip contextMenu)
        {
            // settings
            ToolStripMenuItem settingsMenuItem = new("Settings");
            settingsMenuItem.Click += NotifyClicks.SettingsMenuItem_Click;

            ToolStripSeparator separator= new();

            // exit
            ToolStripMenuItem exitMenuItem = new("Exit");
            exitMenuItem.Click += NotifyClicks.ExitMenuItem_Click;

            contextMenu.Items.Add(settingsMenuItem);
            contextMenu.Items.Add(separator);
            contextMenu.Items.Add(exitMenuItem);
        }

        private void ChannelGroupMenuItems(ContextMenuStrip cms)
        {
            // 壁纸更换间隔设置
            ToolStripMenuItem intervalMenuItem = new("Wallpaper Change interval");
            int[] intervals = { 30, 60, 120 };
            foreach (var minutes in intervals)
            {
                var subItem = new ToolStripMenuItem($"{minutes} minutes");
                subItem.Tag = minutes;
                subItem.Click += NotifyClicks.IntervalMenuItem_Click;
                if (minutes == NotifyClicks.currentInterval)
                    subItem.Checked = true;
                intervalMenuItem.DropDownItems.Add(subItem);
            }
            // separator
            ToolStripSeparator separator = new();

            cms.Items.Add(intervalMenuItem);
            cms.Items.Add(separator);
        }

        private void WallpaperGroupMenuItems(ContextMenuStrip cms)
        {
            // change wallpaper now
            ToolStripMenuItem changeCurrentWallpaperMenuItem = new("Change current wallpaper");
            changeCurrentWallpaperMenuItem.Click += NotifyClicks.ChangeCurrentWallpaperMenuItem_Click;
            cms.Items.Add(changeCurrentWallpaperMenuItem);
            // load previous wallpaper
            ToolStripMenuItem loadPreviousWallpaperMenuItem = new("Load previous wallpaper");
            loadPreviousWallpaperMenuItem.Click += NotifyClicks.LoadPreviousWallpaperMenuItem_Click;
            cms.Items.Add(loadPreviousWallpaperMenuItem);

            ToolStripSeparator separaotr = new();
            cms.Items.Add(separaotr);
        }

        private void SetMenuItemPadding(ToolStripItemCollection items, Padding padding)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.Padding = padding;
                    // 递归设置子菜单
                    if (menuItem.HasDropDownItems)
                    {
                        SetMenuItemPadding(menuItem.DropDownItems, padding);
                    }
                }
            }
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 取消关闭窗口事件
            e.Cancel = true;
            this.Hide(); // 隐藏窗口而不是关闭
            //notifyIcon.ShowBalloonTip(1000, "IrvueWin", "IrvueWin is running in the background.", ToolTipIcon.Info);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // clear notify icon
            if (notifyIcon != null)
            {
                notifyIcon.Dispose();
                notifyIcon = null;
            }
        }
    }
}