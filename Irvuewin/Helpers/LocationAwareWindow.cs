using System.ComponentModel;
using System.Windows;
using Irvuewin.Helpers.Utils;

namespace Irvuewin.Helpers
{
    public class LocationAwareWindow : Window
    {
        /// <summary>
        /// Unique identifier for window position persistence.
        /// Defaults to the class name.
        /// </summary>
        private string PersistenceId => GetType().Name;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowManager.LoadWindowPosition(this, PersistenceId);

            // Apply current theme to title bar
            var isDark = ThemeManager.CurrentTheme == ThemeType.Dark;
            // Note: ThemeManager.CurrentTheme might be System, so we need to check actual state.
            // But ThemeManager doesn't expose strict IsDark boolean publicly easily without logic duplication.
            // Let's rely on checking if current dictionary is Dark.

            // Simpler: Just re-trigger based on ThemeManager state logic
            // Accessing internal/private methods of ThemeManager isn't possible.
            // Let's expose IsDark property or just duplicate simple logic.
            // Or better: ThemeManager.ApplyThemeToWindow(this);

            // Since I haven't added that method to ThemeManager, I will duplicate logic slightly or update ThemeManager.
            // Let's update LocationAwareWindow to use a new public helper property I'll add to ThemeManager or just check Resources.

            var actuallyDark = false;
            try
            {
                actuallyDark = Application.Current.Resources.MergedDictionaries.Any(d =>
                    d.Source != null && d.Source.OriginalString.Contains("Dark"));
            }
            catch
            {
                // ignore   
            }

            WindowThemeHelper.ApplyThemeToWindow(this, actuallyDark);
            UpdateWindowIcon();
        }

        public void UpdateWindowIcon()
        {
            try
            {
                var geometry = Application.Current.FindResource("Icon_AppLogo") as System.Windows.Media.Geometry;
                var brush = Application.Current.FindResource("PrimaryTextBrush") as System.Windows.Media.Brush;
                if (geometry != null && brush != null)
                {
                    // Generate ImageSource for Window Icon (not System.Drawing.Icon)
                    this.Icon = IconHelper.GenerateImageSource(geometry, brush, 24);
                }
            }
            catch (Exception ex)
            {
                // Log error or ignore
                // ignore 
                // System.Diagnostics.Debug.WriteLine($"UpdateWindowIcon Error: {ex.Message}");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            try
            {
                WindowManager.SaveWindowPosition(this, PersistenceId);
            }
            catch
            {
                // Ignore errors during save (e.g. if app data folder was deleted)
            }
        }
    }
}