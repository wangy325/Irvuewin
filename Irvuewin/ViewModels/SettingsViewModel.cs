using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
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

    public SettingsViewModel()
    {
        DisplayModeCheckedCommand = new RelayCommand<object>(OnDisplayModeChecked);
        MultiDisplayCheckedCommand = new RelayCommand<object>(OnMultiDisplayChecked);
        LaunchAtLoginCommand = new RelayCommand<object>(OnLaunchAtLoginChecked);
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
            var cvm = ChannelsViewModel.GetInstance();
            Task.Run(() => cvm.RefreshPhotos(cvm.CheckedChannel));
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

    public ICommand MultiDisplayCheckedCommand { get; }
    public ICommand DisplayModeCheckedCommand { get; }

    public ICommand LaunchAtLoginCommand { get; }

    private void OnMultiDisplayChecked(object obj)
    {
        if (obj is not string tag) return;
        if (byte.TryParse(tag, out var res)) MultiDisplay = res;

        Properties.Settings.Default.MultiDisplay = MultiDisplay;
        Properties.Settings.Default.Save();
    }

    private void OnDisplayModeChecked(object obj)
    {
        if (obj is not string tag) return;
        if (byte.TryParse(tag, out var res)) WallpaperMode = res;

        Properties.Settings.Default.WallpaperMode = WallpaperMode;
        Properties.Settings.Default.Save();
    }

    private void OnLaunchAtLoginChecked(object obj)
    {
        LaunchAtLogin = (bool)obj;
        Properties.Settings.Default.LaunchAtLogin = LaunchAtLogin;
        Properties.Settings.Default.Save();
        StartUpHelper.SetStartup(LaunchAtLogin);
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}