using System.Windows;
using System.Windows.Interop;
using FolderIconStudio.Models;
using FolderIconStudio.Native;

namespace FolderIconStudio.Helpers;

public static class ThemeHelper
{
    public static void ApplyTheme(Window window, AppTheme theme)
    {
        var isDark = theme switch
        {
            AppTheme.Dark => true,
            AppTheme.Light => false,
            _ => IsSystemDark()
        };

        window.Background = System.Windows.Media.Brushes.Transparent;
        ApplyDarkMode(window, isDark);
        ApplyBackdrop(window);
    }

    private static bool IsSystemDark()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int i && i == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void ApplyDarkMode(Window window, bool dark)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
            return;

        var value = dark ? 1 : 0;
        DwmInterop.DwmSetWindowAttribute(hwnd, DwmInterop.DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
    }

    public static void ApplyBackdrop(Window window)
    {
        window.SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                return;

            var backdrop = DwmInterop.DWMSBT_MICA;
            DwmInterop.DwmSetWindowAttribute(hwnd, DwmInterop.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));

            var margins = new DwmInterop.MARGINS { Left = -1, Right = -1, Top = -1, Bottom = -1 };
            DwmInterop.DwmExtendFrameIntoClientArea(hwnd, ref margins);
        };
    }
}
