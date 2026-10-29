using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FolderIconStudio.Models;
using FolderIconStudio.Native;

namespace FolderIconStudio.Services;

public sealed class FolderIconService
{
    private const string DesktopIniName = "desktop.ini";
    private const string IconResourceKey = "IconResource";
    private const string ShellClassSection = ".ShellClassInfo";
    private const string AppMarkerFile = ".foldericonstudio";

    public FolderIconState GetState(string folderPath)
    {
        folderPath = PathHelper.Normalize(folderPath);
        var desktopIniPath = Path.Combine(folderPath, DesktopIniName);
        var markerPath = Path.Combine(folderPath, AppMarkerFile);

        if (!File.Exists(desktopIniPath))
            return new FolderIconState { HasCustomIcon = false };

        var lines = ReadIniLines(desktopIniPath);
        var iconLine = lines.FirstOrDefault(l =>
            l.TrimStart().StartsWith(IconResourceKey + "=", StringComparison.OrdinalIgnoreCase));

        if (iconLine == null)
            return new FolderIconState { HasCustomIcon = false };

        var value = iconLine.Split('=', 2)[1].Trim();
        var iconPath = value.Split(',')[0].Trim();
        var missing = !File.Exists(iconPath);

        return new FolderIconState
        {
            HasCustomIcon = true,
            CurrentIconPath = iconPath,
            IconFileMissing = missing,
            AppSetAttributes = File.Exists(markerPath)
        };
    }

    public void ApplyIcon(string folderPath, string iconPath, bool elevated = false)
    {
        folderPath = PathHelper.Normalize(folderPath);
        iconPath = PathHelper.Normalize(iconPath);

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
        if (!File.Exists(iconPath))
            throw new FileNotFoundException($"Icon not found: {iconPath}");

        _pendingIconPath = iconPath;
        ExecuteWithElevationIfNeeded(folderPath, () =>
        {
            PrepareFolderForWrite(folderPath);
            WriteDesktopIni(folderPath, iconPath);
            SetFolderAttributes(folderPath, system: true, readOnly: true);
            SetDesktopIniHidden(folderPath);
            WriteMarker(folderPath);
            ShellInterop.NotifyFolderChanged(folderPath);
        }, elevated);
    }

    public void RestoreDefault(string folderPath, bool elevated = false)
    {
        folderPath = PathHelper.Normalize(folderPath);
        var desktopIniPath = Path.Combine(folderPath, DesktopIniName);
        var markerPath = Path.Combine(folderPath, AppMarkerFile);
        var state = GetState(folderPath);

        _pendingIconPath = null;
        ExecuteWithElevationIfNeeded(folderPath, () =>
        {
            PrepareFolderForWrite(folderPath);
            if (File.Exists(desktopIniPath))
            {
                var lines = ReadIniLines(desktopIniPath)
                    .Where(l => !l.TrimStart().StartsWith(IconResourceKey + "=", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var hasContent = lines.Any(l => !string.IsNullOrWhiteSpace(l) &&
                    !l.Trim().Equals($"[{ShellClassSection}]", StringComparison.OrdinalIgnoreCase));

                if (hasContent)
                    WriteIniLines(desktopIniPath, lines);
                else
                {
                    TryDelete(desktopIniPath);
                    if (state.AppSetAttributes || File.Exists(markerPath))
                        ClearFolderAttributes(folderPath);
                }
            }
            else if (state.AppSetAttributes)
            {
                ClearFolderAttributes(folderPath);
            }

            TryDelete(markerPath);
            ShellInterop.NotifyFolderChanged(folderPath);
        }, elevated);
    }

    public void PreviewIcon(string folderPath, string iconPath)
    {
        ApplyIcon(folderPath, iconPath);
    }

    private void ExecuteWithElevationIfNeeded(string folderPath, Action action, bool alreadyElevated)
    {
        try
        {
            action();
        }
        catch (UnauthorizedAccessException) when (!alreadyElevated)
        {
            var result = MessageBox.Show(
                $"Access denied when modifying:\n{folderPath}\n\nWould you like to retry with administrator privileges?",
                "Folder Icon Studio",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                RelaunchElevated(folderPath, action);
                return;
            }

            throw;
        }
        catch (IOException ex) when (!alreadyElevated && IsAccessDenied(ex))
        {
            var result = MessageBox.Show(
                $"Cannot write to folder (it may be protected or on a network drive):\n{folderPath}\n\n{ex.Message}\n\nRetry with administrator privileges?",
                "Folder Icon Studio",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                RelaunchElevated(folderPath, action);
                return;
            }

            throw;
        }
        catch (Exception ex) when (alreadyElevated)
        {
            throw new IOException($"Failed to modify folder (even with administrator rights): {ex.Message}", ex);
        }
    }

    private static bool IsAccessDenied(IOException ex) =>
        ex.Message.Contains("access", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("denied", StringComparison.OrdinalIgnoreCase);

    private string? _pendingIconPath;

    public void SetPendingIconForElevation(string iconPath) => _pendingIconPath = iconPath;

    private void RelaunchElevated(string folderPath, Action action)
    {
        var restore = _pendingIconPath == null;
        if (!ElevationHelper.TryRunElevatedAndWait(folderPath, _pendingIconPath, restore, out var error))
        {
            throw new UnauthorizedAccessException(
                error ?? "Administrator privileges are required for this folder.");
        }

        // Elevated child completed successfully; refresh shell from this process too.
        ShellInterop.NotifyFolderChanged(folderPath);
    }

    private static void PrepareFolderForWrite(string folderPath)
    {
        var folderAttrs = FileAttributesInterop.GetAttributes(folderPath);
        if (folderAttrs != uint.MaxValue)
        {
            folderAttrs &= ~FileAttributesInterop.FILE_ATTRIBUTE_READONLY;
            FileAttributesInterop.SetAttributes(folderPath, folderAttrs);
        }

        var desktopIniPath = Path.Combine(folderPath, DesktopIniName);
        if (File.Exists(desktopIniPath))
        {
            FileAttributesInterop.SetAttributes(desktopIniPath, 0x80); // NORMAL
        }

        var markerPath = Path.Combine(folderPath, AppMarkerFile);
        if (File.Exists(markerPath))
        {
            FileAttributesInterop.SetAttributes(markerPath, 0x80);
        }
    }

    private static void WriteDesktopIni(string folderPath, string iconPath)
    {
        var desktopIniPath = Path.Combine(folderPath, DesktopIniName);
        var lines = File.Exists(desktopIniPath) ? ReadIniLines(desktopIniPath).ToList() : [];

        lines.RemoveAll(l => l.TrimStart().StartsWith(IconResourceKey + "=", StringComparison.OrdinalIgnoreCase));

        if (!lines.Any(l => l.Trim().Equals($"[{ShellClassSection}]", StringComparison.OrdinalIgnoreCase)))
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add("");
            lines.Add($"[{ShellClassSection}]");
        }

        var sectionIndex = lines.FindIndex(l => l.Trim().Equals($"[{ShellClassSection}]", StringComparison.OrdinalIgnoreCase));
        lines.Insert(sectionIndex + 1, $"{IconResourceKey}={iconPath},0");

        WriteIniLines(desktopIniPath, lines);
    }

    private static void WriteIniLines(string path, IEnumerable<string> lines)
    {
        var content = string.Join("\r\n", lines);
        if (!content.EndsWith("\r\n"))
            content += "\r\n";

        File.WriteAllText(path, content, System.Text.Encoding.Unicode);
    }

    private static List<string> ReadIniLines(string path)
    {
        if (!File.Exists(path))
            return [];

        var text = File.ReadAllText(path, System.Text.Encoding.Unicode);
        return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();
    }

    private static void SetFolderAttributes(string folderPath, bool system, bool readOnly)
    {
        var attrs = FileAttributesInterop.GetAttributes(folderPath);
        if (attrs == uint.MaxValue)
            attrs = 0;

        if (system)
            attrs |= FileAttributesInterop.FILE_ATTRIBUTE_SYSTEM;
        if (readOnly)
            attrs |= FileAttributesInterop.FILE_ATTRIBUTE_READONLY;

        if (!FileAttributesInterop.SetAttributes(folderPath, attrs))
        {
            var err = Marshal.GetLastWin32Error();
            throw new IOException($"Failed to set folder attributes on {folderPath} (Win32 error {err}).");
        }
    }

    private static void ClearFolderAttributes(string folderPath)
    {
        var attrs = FileAttributesInterop.GetAttributes(folderPath);
        if (attrs == uint.MaxValue)
            return;

        attrs &= ~(FileAttributesInterop.FILE_ATTRIBUTE_SYSTEM | FileAttributesInterop.FILE_ATTRIBUTE_READONLY);
        FileAttributesInterop.SetAttributes(folderPath, attrs);
    }

    private static void SetDesktopIniHidden(string folderPath)
    {
        var desktopIniPath = Path.Combine(folderPath, DesktopIniName);
        var attrs = FileAttributesInterop.GetAttributes(desktopIniPath);
        if (attrs == uint.MaxValue)
            attrs = 0;
        attrs |= FileAttributesInterop.FILE_ATTRIBUTE_HIDDEN | FileAttributesInterop.FILE_ATTRIBUTE_SYSTEM;
        FileAttributesInterop.SetAttributes(desktopIniPath, attrs);
    }

    private static void WriteMarker(string folderPath)
    {
        var markerPath = Path.Combine(folderPath, AppMarkerFile);
        File.WriteAllText(markerPath, DateTime.UtcNow.ToString("O"));
        var attrs = FileAttributesInterop.FILE_ATTRIBUTE_HIDDEN | FileAttributesInterop.FILE_ATTRIBUTE_SYSTEM;
        FileAttributesInterop.SetAttributes(markerPath, attrs);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                FileAttributesInterop.SetAttributes(path, 0x80); // NORMAL
                File.Delete(path);
            }
        }
        catch { /* best effort */ }
    }
}

public static class IconImageHelper
{
    public static ImageSource? LoadIconPreview(string iconPath, int size = 96)
    {
        try
        {
            if (!File.Exists(iconPath))
                return null;

            using var icon = new Icon(iconPath);
            using var bitmap = icon.ToBitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(size, size));
        }
        catch
        {
            return null;
        }
    }

    public static ImageSource? LoadBestSizeIcon(string iconPath, int size)
    {
        try
        {
            if (!File.Exists(iconPath))
                return null;

            using var fs = File.OpenRead(iconPath);
            using var icon = new Icon(fs, new System.Drawing.Size(size, size));
            using var bitmap = icon.ToBitmap();
            var src = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            src.Freeze();
            return src;
        }
        catch
        {
            return LoadIconPreview(iconPath, size);
        }
    }
}
