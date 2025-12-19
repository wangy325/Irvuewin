using System.Reflection;
using Microsoft.Win32;

namespace Irvuewin.Helpers;

/// <summary>
/// Start at login settings class
/// </summary>
public static class StartUpHelper
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private static string AppName => Assembly.GetEntryAssembly()?.GetName().Name ?? "Irvuewin";

    public static bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        var value = key?.GetValue(AppName)?.ToString();
        return !string.IsNullOrEmpty(value);
    }

    public static void SetStartup(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
        if (enabled)
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            key?.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key?.DeleteValue(AppName, false);
        }
    }
}