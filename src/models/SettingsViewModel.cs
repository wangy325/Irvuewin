using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Irvue_win.Properties;
using Irvue_win.src.controls;

namespace Irvue_win.src.models
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        private bool _openSavedWallpaper;
        /// <summary>
        /// 0-same wallpaper, 1-different wallpaler
        /// </summary>
        private byte _multiDisplay;
        /// <summary>
        /// 0-fill, 1-fit, 2-stretch
        /// </summary>
        private byte _wallpaperMode;

        private bool _launchAtLogin;

        private string _wallpaperSavedPath;

        public SettingsViewModel()
        {
            _openSavedWallpaper = Properties.Settings.Default.OpenSavedWallpaper;
            _multiDisplay = Properties.Settings.Default.MultiDisplay;
            _wallpaperMode = Properties.Settings.Default.WallpaperMode;
            _launchAtLogin = Properties.Settings.Default.LaunchAtLogin;
            _wallpaperSavedPath = Properties.Settings.Default.WallpaperSavedPath;
        }

        public bool OpenSavedWallpaper
        {
            get { return _openSavedWallpaper; }
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
            get { return _multiDisplay; }
            set
            {
                if (_multiDisplay != value)
                {
                    _multiDisplay = value;
                    Properties.Settings.Default.MultiDisplay = _multiDisplay;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public byte WallpaperMode {
            get { return _wallpaperMode; }
            set 
            {
                if (_wallpaperMode != value)
                {
                    _wallpaperMode = value;
                    Properties.Settings.Default.WallpaperMode = _wallpaperMode;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool LuanchAtLogin
        {
            get { return _launchAtLogin; }
            set
            {
                if (_launchAtLogin != value)
                {
                    _launchAtLogin = value;
                    Properties.Settings.Default.LaunchAtLogin = _launchAtLogin;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }

        public string WallpaperSavedPath 
        {
            get { return _wallpaperSavedPath; }
            set
            {
                if (_wallpaperSavedPath != value)
                {
                    _wallpaperSavedPath = value;
                    Properties.Settings.Default.WallpaperSavedPath = _wallpaperSavedPath;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();

                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
