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





        private void SetNotifyIcon()
        {
            notifyIcon = new NotifyIcon();

            // icons
            try
            {
                notifyIcon.Icon = new Icon("icons\\application.ico");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loading icon error: {ex.Message}");
                notifyIcon.Icon = SystemIcons.Application;
            }

            // mouse hover prompt
            notifyIcon.Text = "IrvueWin";
            ContextMenuStrip contextMenu = new();

            // settings
            ToolStripMenuItem settingsMenuItem = new("setting");
            settingsMenuItem.Click += SettingsMenuItem_Click;

            // change wallpaper now
            ToolStripMenuItem changeCurrentWallpaperMenuItem = new("change current wallpaper");
            changeCurrentWallpaperMenuItem.Click += ChangeCurrentWallpaperMenuItem_Click;

            // separator line
            ToolStripSeparator separator = new();

            // exit
            ToolStripMenuItem exitMenuItem = new("exit");
            exitMenuItem.Click += ExitMenuItem_Click;

            contextMenu.Items.Add(settingsMenuItem);
            contextMenu.Items.Add(changeCurrentWallpaperMenuItem);
            contextMenu.Items.Add(separator);
            contextMenu.Items.Add(exitMenuItem);

            notifyIcon.ContextMenuStrip = contextMenu;

            // handle mouse click events
            notifyIcon.Click += NotifyIcon_MouseLeftClick;

            notifyIcon.Visible = true;
            System.Windows.Application.Current.Exit += Application_Exit;
        }

        private void NotifyIcon_MouseLeftClick(object? sender, EventArgs e)
        {
            if (e is System.Windows.Forms.MouseEventArgs mouseEventArgs 
                && mouseEventArgs.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
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

        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ChangeCurrentWallpaperMenuItem_Click(object? sender, EventArgs e)
        {
            SetWallPaperButton_Click(null, null);
        }

        private void SettingsMenuItem_Click(object? sender, EventArgs e)
        {
            // TODO open setting window
            System.Windows.MessageBox.Show("Setting window not implement yet~");
        }
    }
}