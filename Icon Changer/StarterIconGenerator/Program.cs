using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: StarterIconGenerator <output-directory>");
    return 1;
}

var outputDir = args[0];
Directory.CreateDirectory(outputDir);

var colors = new (string Name, Color Primary, Color Accent)[]
{
    ("amber", Color.FromArgb(255, 193, 7), Color.FromArgb(255, 160, 0)),
    ("blue", Color.FromArgb(33, 150, 243), Color.FromArgb(25, 118, 210)),
    ("cyan", Color.FromArgb(0, 188, 212), Color.FromArgb(0, 151, 167)),
    ("deep-orange", Color.FromArgb(255, 87, 34), Color.FromArgb(244, 81, 30)),
    ("deep-purple", Color.FromArgb(103, 58, 183), Color.FromArgb(81, 45, 168)),
    ("green", Color.FromArgb(76, 175, 80), Color.FromArgb(56, 142, 60)),
    ("indigo", Color.FromArgb(63, 81, 181), Color.FromArgb(48, 63, 159)),
    ("lime", Color.FromArgb(205, 220, 57), Color.FromArgb(175, 180, 43)),
    ("orange", Color.FromArgb(255, 152, 0), Color.FromArgb(245, 124, 0)),
    ("pink", Color.FromArgb(233, 30, 99), Color.FromArgb(194, 24, 91)),
    ("purple", Color.FromArgb(156, 39, 176), Color.FromArgb(123, 31, 162)),
    ("red", Color.FromArgb(244, 67, 54), Color.FromArgb(211, 47, 47)),
    ("teal", Color.FromArgb(0, 150, 136), Color.FromArgb(0, 121, 107)),
    ("yellow", Color.FromArgb(255, 235, 59), Color.FromArgb(251, 192, 45)),
    ("brown", Color.FromArgb(121, 85, 72), Color.FromArgb(93, 64, 55)),
    ("blue-grey", Color.FromArgb(96, 125, 139), Color.FromArgb(69, 90, 100)),
    ("coral", Color.FromArgb(255, 111, 97), Color.FromArgb(230, 74, 59)),
    ("forest", Color.FromArgb(46, 125, 50), Color.FromArgb(27, 94, 32)),
    ("gold", Color.FromArgb(255, 193, 7), Color.FromArgb(212, 160, 23)),
    ("lavender", Color.FromArgb(179, 157, 219), Color.FromArgb(149, 117, 205)),
    ("mint", Color.FromArgb(102, 187, 106), Color.FromArgb(67, 160, 71)),
    ("navy", Color.FromArgb(26, 35, 126), Color.FromArgb(13, 71, 161)),
    ("rose", Color.FromArgb(240, 98, 146), Color.FromArgb(216, 27, 96)),
    ("slate", Color.FromArgb(69, 90, 100), Color.FromArgb(38, 50, 56)),
    ("sky", Color.FromArgb(3, 169, 244), Color.FromArgb(2, 136, 209)),
    ("violet", Color.FromArgb(142, 36, 170), Color.FromArgb(106, 27, 154)),
};

var sizes = new[] { 16, 24, 32, 48, 64, 128, 256 };

foreach (var (name, primary, accent) in colors)
{
    var path = Path.Combine(outputDir, $"folder-{name}.ico");
    using var icon = CreateFolderIcon(primary, accent, sizes);
    using var fs = File.Create(path);
    icon.Save(fs);
}

Console.WriteLine($"Generated {colors.Length} starter icons in {outputDir}");
return 0;

static Icon CreateFolderIcon(Color primary, Color accent, int[] sizes)
{
    var images = new List<Bitmap>();
    foreach (var size in sizes)
    {
        images.Add(DrawFolder(size, primary, accent));
    }

    using var ms = new MemoryStream();
    WriteIco(ms, images);
    ms.Position = 0;
    return new Icon(ms);
}

static Bitmap DrawFolder(int size, Color primary, Color accent)
{
    var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.Clear(Color.Transparent);

    var scale = size / 256f;
    var bodyRect = new RectangleF(24 * scale, 80 * scale, 208 * scale, 144 * scale);
    var tabRect = new RectangleF(24 * scale, 48 * scale, 96 * scale, 48 * scale);
    var tabTop = new RectangleF(48 * scale, 32 * scale, 72 * scale, 32 * scale);

    using var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
    g.FillRoundedRect(new RectangleF(bodyRect.X + 4 * scale, bodyRect.Y + 6 * scale, bodyRect.Width, bodyRect.Height), 12 * scale, shadowBrush);

    using var accentBrush = new LinearGradientBrush(
        new PointF(tabTop.Left, tabTop.Top), new PointF(tabTop.Left, tabTop.Bottom),
        accent, Darken(accent, 0.1f));
    using var bodyBrush = new LinearGradientBrush(
        new PointF(bodyRect.Left, bodyRect.Top), new PointF(bodyRect.Left, bodyRect.Bottom),
        primary, Darken(primary, 0.15f));

    var path = new GraphicsPath();
    path.AddRoundedRect(tabTop, 8 * scale);
    path.AddRoundedRect(tabRect, 8 * scale);
    path.AddRoundedRect(bodyRect, 14 * scale);
    g.FillPath(accentBrush, path);
    g.FillRoundedRect(bodyRect, 14 * scale, bodyBrush);

    using var highlight = new Pen(Color.FromArgb(60, 255, 255, 255), Math.Max(1f, 2 * scale));
    g.DrawLine(highlight, bodyRect.Left + 16 * scale, bodyRect.Top + 8 * scale, bodyRect.Right - 16 * scale, bodyRect.Top + 8 * scale);

    return bmp;
}

static void WriteIco(Stream stream, List<Bitmap> images)
{
    using var bw = new BinaryWriter(stream, System.Text.Encoding.Default, leaveOpen: true);
    bw.Write((short)0);
    bw.Write((short)1);
    bw.Write((short)images.Count);

    var offset = 6 + 16 * images.Count;
    var pngData = new List<byte[]>();
    foreach (var img in images)
    {
        using var ms = new MemoryStream();
        img.Save(ms, ImageFormat.Png);
        pngData.Add(ms.ToArray());
    }

    for (var i = 0; i < images.Count; i++)
    {
        var img = images[i];
        bw.Write((byte)(img.Width >= 256 ? 0 : img.Width));
        bw.Write((byte)(img.Height >= 256 ? 0 : img.Height));
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((short)1);
        bw.Write((short)32);
        bw.Write(pngData[i].Length);
        bw.Write(offset);
        offset += pngData[i].Length;
    }

    foreach (var data in pngData)
        bw.Write(data);
}

static Color Darken(Color color, float amount)
{
    var r = (int)(color.R * (1 - amount));
    var g = (int)(color.G * (1 - amount));
    var b = (int)(color.B * (1 - amount));
    return Color.FromArgb(color.A, r, g, b);
}

static class GraphicsExtensions
{
    public static void FillRoundedRect(this Graphics g, RectangleF rect, float radius, Brush brush)
    {
        using var path = new GraphicsPath();
        path.AddRoundedRect(rect, radius);
        g.FillPath(brush, path);
    }

    public static void AddRoundedRect(this GraphicsPath path, RectangleF rect, float radius)
    {
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
    }
}
