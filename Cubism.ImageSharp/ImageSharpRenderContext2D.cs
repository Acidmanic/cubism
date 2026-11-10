using Cubism.Abstractions;
using Cubism.Models;
using Cubism.Visual;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Cubism.ImageSharp;

public class ImageSharpRenderContext2D : IRenderContext2D
{
    private readonly Image _image;
    private readonly Dictionary<System.Drawing.Color, Color> _colorMap = new();
    private readonly Dictionary<(int Argb, double Thickness), SolidPen> _solidPensMap = new();
    private readonly Dictionary<System.Drawing.Color, SolidBrush> _solidBrushesMap = new();

    public ImageSharpRenderContext2D(Image image)
    {
        _image = image;
    }

    public void DrawLine(System.Drawing.Color color, double thickness, Point2 p1, Point2 p2)
    {
        var pen = MapSolidPenCached(color, thickness);
        _image.Mutate(ctx => ctx.DrawLine(pen, new[] { Map(p1), Map(p2) }));
    }

    public void DrawPolygon(System.Drawing.Color color, double thickness, params Point2[] points)
    {
        if (points.Length < 2)
            return;

        var pen = MapSolidPenCached(color, thickness);
        var mapped = Map(points);
        var closed = new PointF[mapped.Length + 1];
        Array.Copy(mapped, closed, mapped.Length);
        closed[^1] = mapped[0];
        _image.Mutate(ctx => ctx.DrawLine(pen, closed));
    }

    public void FillPolygon(System.Drawing.Color color, params Point2[] points)
    {
        if (points.Length < 3)
            return;

        var brush = MapSolidBrushCached(color);
        _image.Mutate(ctx => ctx.FillPolygon(brush, Map(points)));
    }

    public void DrawImage(Bitmap2D image, Point2 topLeft, double width, double height)
    {
        if (width < 1 || height < 1)
            return;

        int w = Math.Max(1, (int)Math.Round(width));
        int h = Math.Max(1, (int)Math.Round(height));
        using var source = Image.LoadPixelData<Rgba32>(image.Rgba, image.Width, image.Height);
        source.Mutate(ctx => ctx.Resize(w, h));
        var location = new Point((int)Math.Round(topLeft.X), (int)Math.Round(topLeft.Y));
        _image.Mutate(ctx => ctx.DrawImage(source, location, 1f));
    }

    private Color MapColorCached(System.Drawing.Color color)
    {
        if (_colorMap.TryGetValue(color, out var value))
            return value;

        var mapped = Color.FromPixel(new Argb32(color.R, color.G, color.B, color.A));
        _colorMap.Add(color, mapped);
        return mapped;
    }

    private SolidPen MapSolidPenCached(System.Drawing.Color color, double thickness)
    {
        var key = (color.ToArgb(), thickness);
        if (_solidPensMap.TryGetValue(key, out var value))
            return value;

        var pen = new SolidPen(MapColorCached(color), (float)thickness);
        _solidPensMap.Add(key, pen);
        return pen;
    }

    private SolidBrush MapSolidBrushCached(System.Drawing.Color color)
    {
        if (_solidBrushesMap.TryGetValue(color, out var value))
            return value;

        var brush = new SolidBrush(MapColorCached(color));
        _solidBrushesMap.Add(color, brush);
        return brush;
    }

    private static PointF Map(Point2 point) => new((float)point.X, (float)point.Y);

    private static PointF[] Map(Point2[] points) => points.Select(Map).ToArray();
}
