using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using FolderIconStudio.Models;

namespace FolderIconStudio.Services;

public sealed class IconLibraryService : IDisposable
{
    private static readonly int[] IconSizes = [16, 24, 32, 48, 64, 128, 256];
    private FileSystemWatcher? _watcher;
    private string _libraryPath = "";

    public event EventHandler? LibraryChanged;

    public string LibraryPath => _libraryPath;

    public void Initialize(string libraryPath)
    {
        _libraryPath = Path.GetFullPath(libraryPath);
        Directory.CreateDirectory(_libraryPath);
        EnsureStarterIcons();
        StartWatcher();
    }

    public void UpdatePath(string newPath)
    {
        _watcher?.Dispose();
        _watcher = null;
        _libraryPath = Path.GetFullPath(newPath);
        Directory.CreateDirectory(_libraryPath);
        StartWatcher();
        LibraryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void StartWatcher()
    {
        _watcher = new FileSystemWatcher(_libraryPath, "*.ico")
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
        };
        _watcher.Created += OnWatcherEvent;
        _watcher.Changed += OnWatcherEvent;
        _watcher.Deleted += OnWatcherEvent;
        _watcher.Renamed += (_, _) => LibraryChanged?.Invoke(this, EventArgs.Empty);
        _watcher.EnableRaisingEvents = true;
    }

    private void OnWatcherEvent(object sender, FileSystemEventArgs e)
    {
        // Debounce slightly
        Task.Delay(300).ContinueWith(_ => LibraryChanged?.Invoke(this, EventArgs.Empty));
    }

    public IReadOnlyList<IconItem> GetAllIcons(SettingsService settings)
    {
        var items = new Dictionary<string, IconItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in Directory.EnumerateFiles(_libraryPath, "*.ico"))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            items[path] = new IconItem
            {
                FilePath = path,
                DisplayName = name,
                IsStarter = name.StartsWith("folder-", StringComparison.OrdinalIgnoreCase),
                IsFavorite = settings.IsFavorite(path)
            };
        }

        return items.Values.OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public string ImportImage(string sourcePath)
    {
        sourcePath = Path.GetFullPath(sourcePath);
        var ext = Path.GetExtension(sourcePath).ToLowerInvariant();
        var baseName = Path.GetFileNameWithoutExtension(sourcePath);
        var safeName = SanitizeFileName(baseName);
        var destPath = GetUniquePath(Path.Combine(_libraryPath, safeName + ".ico"));

        if (ext == ".ico")
        {
            File.Copy(sourcePath, destPath, overwrite: false);
        }
        else
        {
            ConvertToMultiSizeIco(sourcePath, destPath);
        }

        LibraryChanged?.Invoke(this, EventArgs.Empty);
        return destPath;
    }

    public static void ConvertToMultiSizeIco(string imagePath, string outputIcoPath)
    {
        using var source = LoadImage(imagePath);
        var bitmaps = new List<Bitmap>();

        foreach (var size in IconSizes)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            var srcAspect = (float)source.Width / source.Height;
            float drawW, drawH, drawX, drawY;
            if (srcAspect > 1)
            {
                drawW = size;
                drawH = size / srcAspect;
                drawX = 0;
                drawY = (size - drawH) / 2;
            }
            else
            {
                drawH = size;
                drawW = size * srcAspect;
                drawX = (size - drawW) / 2;
                drawY = 0;
            }

            g.DrawImage(source, drawX, drawY, drawW, drawH);
            bitmaps.Add(bmp);
        }

        WriteIcoFile(outputIcoPath, bitmaps);
        foreach (var bmp in bitmaps)
            bmp.Dispose();
    }

    private static Image LoadImage(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext == ".svg")
        {
            // Basic SVG rasterization fallback: try to load via System.Drawing (won't work for SVG)
            // For v1, copy a placeholder or use Skia - we'll use a simple approach with Bitmap from file if possible
            throw new NotSupportedException(
                "SVG import requires the image to be converted. Please export as PNG first, or use PNG/JPG/ICO.");
        }

        return Image.FromFile(path);
    }

    private static void WriteIcoFile(string path, List<Bitmap> images)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        bw.Write((short)0);
        bw.Write((short)1);
        bw.Write((short)images.Count);

        var pngDataList = new List<byte[]>();
        foreach (var img in images)
        {
            using var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            pngDataList.Add(ms.ToArray());
        }

        var offset = 6 + 16 * images.Count;
        for (var i = 0; i < images.Count; i++)
        {
            var w = images[i].Width;
            var h = images[i].Height;
            bw.Write((byte)(w >= 256 ? 0 : w));
            bw.Write((byte)(h >= 256 ? 0 : h));
            bw.Write((byte)0);
            bw.Write((byte)0);
            bw.Write((short)1);
            bw.Write((short)32);
            bw.Write(pngDataList[i].Length);
            bw.Write(offset);
            offset += pngDataList[i].Length;
        }

        foreach (var data in pngDataList)
            bw.Write(data);
    }

    private void EnsureStarterIcons()
    {
        var assetsDir = Path.Combine(AppContext.BaseDirectory, "Assets", "StarterIcons");
        if (!Directory.Exists(assetsDir))
            return;

        foreach (var ico in Directory.EnumerateFiles(assetsDir, "*.ico"))
        {
            var dest = Path.Combine(_libraryPath, Path.GetFileName(ico));
            if (!File.Exists(dest))
                File.Copy(ico, dest);
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "imported-icon" : name;
    }

    private static string GetUniquePath(string path)
    {
        if (!File.Exists(path))
            return path;

        var dir = Path.GetDirectoryName(path)!;
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        var i = 1;
        while (File.Exists(path))
            path = Path.Combine(dir, $"{name}-{i++}{ext}");
        return path;
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
