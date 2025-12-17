using System.IO;
using System.Windows;
using H.NotifyIcon;
using Irvuewin.Helpers;
using Irvuewin.ViewModels;
using Serilog;
using Localization = Irvuewin.Helpers.Localization;

namespace Irvuewin.Views;

public partial class Settings
{
    private static readonly ILogger  Logger = Log.ForContext(typeof(Settings)); 
    public Settings()
    {
        InitializeComponent();
        //this.DataContext = this;
    }


    private void BrowseWallpaperSavePathButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
        {
            Title = Localization.Instance["Choose_Folder"],
            IsFolderPicker = true,
            InitialDirectory = IAppConst.DefaultWallpaperDownloadDir,
            ShowHiddenItems = false,
        };

        // 指定 owner，防止任务栏出现新图标
        var result = dialog.ShowDialog(this);

        if (result != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) return;
        var selectedPath = dialog.FileName;
        if (selectedPath == null) return;
        var systemFolders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.System),
        };

        // Exclude system folders, hidden folders, and root system folders.
        if (systemFolders.Any(sf => selectedPath.StartsWith(sf, StringComparison.OrdinalIgnoreCase))
            || (File.GetAttributes(selectedPath) & FileAttributes.Hidden) == FileAttributes.Hidden
            || selectedPath.Equals(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))))
        {
            MessageBoxWindow.Show(
                Localization.Instance["Folder_Error"],
                Localization.Instance["Msg_Error"],
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }
        // WallpaperSavePathTextBox.Text = selectedPath;

        var settingsViewModel = this.DataContext as SettingsViewModel;
        settingsViewModel!.WallpaperSavedPath = selectedPath;

        // PathDisplayControl.FullPath = selectedPath;
        // 更新回显
        /*if (PathDisplayControl.ToolTip is ToolTip { Content: TextBlock textBlock })
            {
                textBlock.Text = selectedPath;
            }*/

        Properties.Settings.Default.WallpaperSavedPath = selectedPath;
        Properties.Settings.Default.Save();
    }

    /// <summary>
    /// SAVE BUTTON
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        // UNDONE: Get and save page data

        var _TaskBarIcon = (TaskbarIcon)FindResource("NotifyIcon");
        // _TaskBarIcon.ShowBalloonTip("INFO", "Saved Settings", BalloonIcon.Info);
    }

    /// <summary>
    /// CANCEL BUTTON
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CancelSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }

    private void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBoxWindow.Show(
            Localization.Instance["Settings_ClearCache_Confirm"],
            Localization.Instance["Msg_Hint"],
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Irvuewin", "splash");
            Logger.Debug(@"Deleting wallpaper cache {cachePath}",  cachePath);
            // Delete cache logic
            // if (Directory.Exists(cachePath))
            // {
            //     Directory.Delete(cachePath, true);
            // }
        }
    }

    private void ResetApp_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBoxWindow.Show(
            Localization.Instance["Settings_ResetApp_Confirm"],
            Localization.Instance["Msg_Hint"],
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Irvuewin");
            Logger.Debug(@"Reset app {appDataPath}",  appDataPath);
            // Reset logic
            // if (Directory.Exists(appDataPath))
            // {
            //      Directory.Delete(appDataPath, true);
            // }
            // System.Windows.Forms.Application.Restart();
            // Application.Current.Shutdown();
        }
    }
}