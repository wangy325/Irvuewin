using System.IO;
using System.Windows;
using Irvuewin.Helpers;
using Irvuewin.ViewModels;
using Serilog;
using Localization = Irvuewin.Helpers.Localization;

namespace Irvuewin.Views;

public partial class Settings
{
    // private static readonly ILogger  Logger = Log.ForContext<Settings>(); 
    public Settings()
    {
        InitializeComponent();
        //this.DataContext = this;
    }


    private void BrowseWallpaperSavePathButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
        {
            Title = Localization.Instance["Settings_Choose_Folder"],
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
                Localization.Instance["Settings_Folder_Error"],
                Localization.Instance["Msg_Error"],
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }
        // WallpaperSavePathTextBox.Text = selectedPath;

        var settingsViewModel = DataContext as SettingsViewModel;
        settingsViewModel!.WallpaperSavedPath = selectedPath;
        // Save user settings
        Properties.Settings.Default.WallpaperSavedPath = selectedPath;
        Properties.Settings.Default.Save();
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
            // Delete cache logic
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
            }
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
            // Logger.Debug(@"Reset app {appDataPath}",  appDataPath);
            
            // Close and flush the logger to release the lock on the log file
            Log.CloseAndFlush();
            
            // Reset logic
            if (Directory.Exists(appDataPath))
            {
                try
                {
                    Directory.Delete(appDataPath, true);
                }
                catch (Exception)
                {
                    // Ignore exceptions (e.g., if some files are still locked)
                    // The app will restart and can handle partial state or overwrite on next run
                }
            }
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }
    }
}