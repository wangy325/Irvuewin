using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Irvuewin.Helpers
{
    public static class WindowThemeHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void ApplyThemeToWindow(Window window, bool isDark)
        {
            if (window == null) return;

            var windowHandle = new WindowInteropHelper(window).Handle;
            if (windowHandle == IntPtr.Zero) return;

            int useDarkMode = isDark ? 1 : 0;
            
            // Try newer attribute first
            if (DwmSetWindowAttribute(windowHandle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int)) != 0)
            {
                // Fallback for older Windows 10 versions
                DwmSetWindowAttribute(windowHandle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDarkMode, sizeof(int));
            }
        }

        public static void RefreshAllWindows(bool isDark)
        {
            foreach (Window window in Application.Current.Windows)
            {
                ApplyThemeToWindow(window, isDark);
                if (window is LocationAwareWindow law)
                {
                    law.UpdateWindowIcon();
                }
            }
        }
    }
}
