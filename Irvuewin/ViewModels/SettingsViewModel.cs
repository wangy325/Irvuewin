using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;

namespace Irvuewin.ViewModels;

///<summary>
///Author: wangy325
///Date: 2020/01/01 18:18:18
///Desc: 
///</summary>
public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _openSavedWallpaper = Properties.Settings.Default.OpenSavedWallpaper;

    /// <summary>
    /// 0-same wallpaper, 1-different wallpaper
    /// </summary>
    private byte _multiDisplay = Properties.Settings.Default.MultiDisplay;

    /// <summary>
    /// 0-fill, 1-fit, 2-center
    /// </summary>
    private byte _wallpaperMode = Properties.Settings.Default.WallpaperMode;

    private bool _launchAtLogin = Properties.Settings.Default.LaunchAtLogin;

    private string _wallpaperSavedPath = Properties.Settings.Default.WallpaperSavedPath;

    /// <summary>
    /// 0-landscape, 1-portrait, 2-both
    /// </summary>
    private byte _wallpaperOrientation = Properties.Settings.Default.WallpaperOrientation;

    /// <summary>
    /// 0-none 1-face 2-face and body
    /// </summary>
    private byte _smartFilter = Properties.Settings.Default.SmartFilter;

    /// <summary>
    /// ratio of system display resolution
    /// </summary>
    private float _minResolution = Properties.Settings.Default.MinResolution;
    
    public ICommand MultiDisplayCheckedCommand { get; }
    public ICommand DisplayModeCheckedCommand { get; }
    public ICommand LaunchAtLoginCommand { get; }
    public ICommand AsyncRefreshWallpaperCommand { get; }

    public SettingsViewModel()
    {
        DisplayModeCheckedCommand = new RelayCommand<string>(OnDisplayModeChecked);
        MultiDisplayCheckedCommand = new RelayCommand<string>(OnMultiDisplayChecked);
        LaunchAtLoginCommand = new RelayCommand<bool>(OnLaunchAtLoginChecked);
        AsyncRefreshWallpaperCommand = new AsyncRelayCommand(OnOrientationChanged);
    }

    public bool OpenSavedWallpaper
    {
        get => _openSavedWallpaper;
        set
        {
            _openSavedWallpaper = value;
            Properties.Settings.Default.OpenSavedWallpaper = _openSavedWallpaper;
            Properties.Settings.Default.Save();
            OnPropertyChanged();
        }
    }

    public byte MultiDisplay
    {
        get => _multiDisplay;
        set
        {
            if (_multiDisplay == value) return;
            _multiDisplay = value;
            OnPropertyChanged();
        }
    }

    public byte WallpaperMode
    {
        get => _wallpaperMode;
        set
        {
            if (_wallpaperMode == value) return;
            _wallpaperMode = value;
            OnPropertyChanged();
        }
    }

    public bool LaunchAtLogin
    {
        get => _launchAtLogin;
        set
        {
            if (_launchAtLogin == value) return;
            _launchAtLogin = value;
            // Properties.Settings.Default.LaunchAtLogin = _launchAtLogin;
            // Properties.Settings.Default.Save();
            OnPropertyChanged();
        }
    }

    public string WallpaperSavedPath
    {
        get => string.IsNullOrWhiteSpace(_wallpaperSavedPath)
            ? IAppConst.DefaultWallpaperDownloadDir
            : _wallpaperSavedPath;
        set
        {
            if (_wallpaperSavedPath == value) return;
            _wallpaperSavedPath = value;
            OnPropertyChanged();
        }
    }

    public byte WallpaperOrientation
    {
        get => _wallpaperOrientation;
        set
        {
            if (_wallpaperOrientation == value) return;
            _wallpaperOrientation = value;
            Properties.Settings.Default.WallpaperOrientation = _wallpaperOrientation;
            Properties.Settings.Default.Save();
            OnPropertyChanged();
            // var cvm = ChannelsViewModel.GetInstance();
            // Task.Run(() => cvm.RefreshPhotos(cvm.CheckedChannelId));
            if (AsyncRefreshWallpaperCommand.CanExecute(null))
            {
                AsyncRefreshWallpaperCommand.Execute(null);
            }
        }
    }

    public byte SmartFilter
    {
        get => _smartFilter;
        set
        {
            if (_smartFilter == value) return;
            _smartFilter = value;
            Properties.Settings.Default.SmartFilter = _smartFilter;
            Properties.Settings.Default.Save();
            OnPropertyChanged();
        }
    }

    public float MinResolution
    {
        get => _minResolution;
        set
        {
            const double tolerance = 0.0001;
            if (Math.Abs(_minResolution - value) < tolerance) return;
            _minResolution = value;
            Properties.Settings.Default.MinResolution = _minResolution;
            Properties.Settings.Default.Save();
            OnPropertyChanged();
        }
    }

    private string _language = Properties.Settings.Default.Language;

    public string Language
    {
        get => _language;
        set
        {
            if (_language == value) return;
            _language = value;
            Properties.Settings.Default.Language = _language;
            Properties.Settings.Default.Save();

            // Apply culture immediately via Localization helper
            Localization.Instance.SetCulture(_language);

            OnPropertyChanged();
        }
    }
    
    
    // Default to 2 (Auto) if not saved
    private int _theme = Properties.Settings.Default.Theme; 

    public int Theme
    {
        get => _theme;
        set
        {
            if (_theme == value) return;
            _theme = value;
            Properties.Settings.Default.Theme = _theme; 
            Properties.Settings.Default.Save();

            ThemeManager.SetTheme((ThemeType)_theme);
            OnPropertyChanged();
        }
    }

    private void OnMultiDisplayChecked(string str)
    {
        if (byte.TryParse(str, out var res)) MultiDisplay = res;

        Properties.Settings.Default.MultiDisplay = MultiDisplay;
        Properties.Settings.Default.Save();
    }

    private void OnDisplayModeChecked(string str)
    {
        if (byte.TryParse(str, out var res)) WallpaperMode = res;

        Properties.Settings.Default.WallpaperMode = WallpaperMode;
        Properties.Settings.Default.Save();
    }

    private void OnLaunchAtLoginChecked(bool flag)
    {
        Properties.Settings.Default.LaunchAtLogin = flag;
        Properties.Settings.Default.Save();
        StartUpHelper.SetStartup(LaunchAtLogin);
    }

    private async Task OnOrientationChanged()
    {
        await IrvuewinCore.RefreshAllCachedWallpapers();
    }


    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}