using System.Runtime.InteropServices;
using System.Text;

namespace FolderIconStudio.Native;

internal static class ShellInterop
{
    public const uint SHCNE_ASSOCCHANGED = 0x08000000;
    public const uint SHCNE_UPDATEIMAGE = 0x00008000;
    public const uint SHCNE_UPDATEDIR = 0x00001000;
    public const uint SHCNE_ATTRIBUTES = 0x00002000;
    public const uint SHCNF_PATHW = 0x0005;
    public const uint SHCNF_FLUSH = 0x1000;
    public const uint SHCNF_FLUSHNOWAIT = 0x3000;
    public const uint SHCNF_IDLIST = 0x0000;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern void SHChangeNotify(uint wEventId, uint uFlags, string? dwItem1, string? dwItem2);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, uint dwFlags, StringBuilder pszPath);

    public static void NotifyFolderChanged(string folderPath)
    {
        var parent = Path.GetDirectoryName(folderPath);
        SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATHW | SHCNF_FLUSHNOWAIT, folderPath, null);
        if (!string.IsNullOrEmpty(parent))
            SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATHW | SHCNF_FLUSHNOWAIT, parent, null);
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        SHChangeNotify(SHCNE_UPDATEIMAGE, SHCNF_FLUSHNOWAIT, IntPtr.Zero, IntPtr.Zero);
    }
}

internal static class FileAttributesInterop
{
    public const uint FILE_ATTRIBUTE_READONLY = 0x1;
    public const uint FILE_ATTRIBUTE_HIDDEN = 0x2;
    public const uint FILE_ATTRIBUTE_SYSTEM = 0x4;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool SetFileAttributes(string lpFileName, uint dwFileAttributes);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern uint GetFileAttributes(string lpFileName);

    public static bool SetAttributes(string path, uint attributes)
    {
        var longPath = ToLongPath(path);
        if (SetFileAttributes(longPath, attributes))
            return true;

        return SetFileAttributes(Path.GetFullPath(path), attributes);
    }

    public static uint GetAttributes(string path)
    {
        var longPath = ToLongPath(path);
        return GetFileAttributes(longPath);
    }

    public static string ToLongPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;
        path = Path.GetFullPath(path);
        if (path.StartsWith(@"\\?\"))
            return path;
        if (path.StartsWith(@"\\"))
            return @"\\?\UNC\" + path[2..];
        return @"\\?\" + path;
    }
}

internal static class DwmInterop
{
    public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    public const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    public const int DWMWA_MICA_EFFECT = 1029;

    public const int DWMSBT_AUTO = 0;
    public const int DWMSBT_MICA = 2;
    public const int DWMSBT_ACRYLIC = 3;
    public const int DWMSBT_TABBED = 4;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("dwmapi.dll")]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int Left, Right, Top, Bottom;
    }
}

internal static class PathHelper
{
    public static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;
        return Path.GetFullPath(path.Trim().Trim('"'));
    }

    public static bool IsValidDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;
        try
        {
            path = Normalize(path);
            return Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }
}
