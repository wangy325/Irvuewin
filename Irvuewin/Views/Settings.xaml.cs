using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Irvuewin.Controls;
using Irvuewin.Helpers;
using Irvuewin.ViewModels;

namespace Irvuewin.Views;

public partial class Settings : LocationAwareWindow
{
    public Settings()
    {
        InitializeComponent();
        //this.DataContext = this;
    }


    private void BrowseWallpaperSavePathButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
        {
            Title = IAppConst.ChooseFolder,
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
            MessageBox.Show(
                IAppConst.Hints.IllegalFolder,
                IAppConst.Hints.MessageBoxCaption,
                MessageBoxButton.OK
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
        _TaskBarIcon.ShowBalloonTip("INFO", "Saved Settings", BalloonIcon.Info);
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
}