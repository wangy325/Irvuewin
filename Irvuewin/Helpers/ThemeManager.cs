using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Irvuewin.Helpers
{
    public enum ThemeType
    {
        Light = 0,
        Dark = 1,
        System = 2
    }

    public static class ThemeManager
    {
        public static ThemeType CurrentTheme { get; private set; } = ThemeType.Light;
        private static bool _isListening = false;

        public static void SetTheme(ThemeType theme)
        {
            CurrentTheme = theme;

            if (theme == ThemeType.System)
            {
                if (!_isListening)
                {
                    SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
                    _isListening = true;
                }
                ApplyTheme(GetSystemTheme());
            }
            else
            {
                if (_isListening)
                {
                    SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
                    _isListening = false;
                }
                ApplyTheme(theme);
            }
        }

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (CurrentTheme == ThemeType.System)
                    {
                        ApplyTheme(GetSystemTheme());
                    }
                });
            }
        }

        private static ThemeType GetSystemTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("AppsUseLightTheme");
                        if (val is int i && i == 0)
                        {
                            return ThemeType.Dark;
                        }
                    }
                }
            }
            catch { }
            return ThemeType.Light;
        }

        private static void ApplyTheme(ThemeType theme)
        {
            string themeUri = null;
            switch (theme)
            {
                case ThemeType.Light: // Fallback for System returning Light
                    themeUri = "/Themes/Colors.xaml";
                    break;
                case ThemeType.Dark:
                    themeUri = "/Themes/Colors.Dark.xaml";
                    break;
            }

            if (string.IsNullOrEmpty(themeUri)) return;

            try
            {
                // Remove existing theme dictionaries
                var mergedDicts = Application.Current.Resources.MergedDictionaries;
                var existingTheme = mergedDicts.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("/Themes/Colors.xaml") || d.Source.OriginalString.Contains("/Themes/Colors.Dark.xaml")));
                
                if (existingTheme != null)
                {
                    mergedDicts.Remove(existingTheme);
                }
                
                // Add new theme
                mergedDicts.Add(new ResourceDictionary() { Source = new Uri(themeUri, UriKind.Relative) });

                // Apply Title Bar Theme
                bool isDark = (theme == ThemeType.Dark || (theme == ThemeType.System && GetSystemTheme() == ThemeType.Dark));
                WindowThemeHelper.RefreshAllWindows(isDark);
                if (Application.Current is App app)
                {
                    app.RefreshTrayIcon();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing theme: {ex.Message}");
            }
        }
    }
}
