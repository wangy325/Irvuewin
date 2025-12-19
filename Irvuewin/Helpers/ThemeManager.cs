using Microsoft.Win32;
using System.Windows;

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
        private static bool _isListening;

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
                using var key =
                    Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var val = key?.GetValue("AppsUseLightTheme");
                if (val is 0)
                {
                    return ThemeType.Dark;
                }
            }
            catch
            {
                // ignore
            }
            return ThemeType.Light;
        }

        private static void ApplyTheme(ThemeType theme)
        {
            var themeUri = theme switch
            {
                ThemeType.Light => // Fallback for System returning Light
                    "/Themes/Colors.xaml",
                ThemeType.Dark => "/Themes/Colors.Dark.xaml",
                _ => null
            };

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
                var isDark = (theme == ThemeType.Dark || (theme == ThemeType.System && GetSystemTheme() == ThemeType.Dark));
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
