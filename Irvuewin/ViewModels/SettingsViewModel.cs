using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Irvuewin.ViewModels;

///<summary>
///Author: wangy325
///Date: 2020/01/01 18:18:18
///Desc: 
///</summary>
public class SettingsViewModel: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

        private bool _openSavedWallpaper = Properties.Settings.Default.OpenSavedWallpaper;

        /// <summary>
        /// 0-same wallpaper, 1-different wallpaper
        /// </summary>
        private byte _multiDisplay = Properties.Settings.Default.MultiDisplay;

        /// <summary>
        /// 0-fill, 1-fit, 2-stretch
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
                Properties.Settings.Default.MultiDisplay = _multiDisplay;
                Properties.Settings.Default.Save();
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
                Properties.Settings.Default.WallpaperMode = _wallpaperMode;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"WallpaperMode saved: {_wallpaperMode}");
            }
        }

        public bool LaunchAtLogin
        {
            get => _launchAtLogin;
            set
            {
                if (_launchAtLogin == value) return;
                _launchAtLogin = value;
                Properties.Settings.Default.LaunchAtLogin = _launchAtLogin;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public string WallpaperSavedPath
        {
            get => _wallpaperSavedPath;
            set
            {
                if (_wallpaperSavedPath == value) return;
                _wallpaperSavedPath = value;
                Properties.Settings.Default.WallpaperSavedPath = _wallpaperSavedPath;
                Properties.Settings.Default.Save();
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
}