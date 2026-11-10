using Cubism.Abstractions;
using Cubism.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace Cubism.ImageSharp;

public class ImageSharpRenderContext2D : IRenderContext2D
{
    private class PenKey
    {
        public PenKey(System.Drawing.Color color, double thickness)
        {
            Thickness = thickness;
            Color = color;

            Key = $"{Thickness:00.0000}:{Color.ToArgb()}";
        }

        public double Thickness { get; }

        public System.Drawing.Color Color { get; }

        public string Key { get; }
    }

    private readonly Image _image;
    private Dictionary<System.Drawing.Color, Color> _colorMap = new();
    private Dictionary<PenKey, SolidPen> _solidPensMap = new();
    private Dictionary<System.Drawing.Color, SolidBrush> _solidBrushesMap = new();

    public ImageSharpRenderContext2D(Image image)
    {
        _image = image;
    }

    public void DrawLine(System.Drawing.Color color, double thickness, Point2 p1, Point2 p2)
    {
        var pen = mapSolidPenCached(color, thickness);

        _image.Mutate(x => { x.Paint(c => c.DrawLine(pen, map([p1, p2]))); });
    }

    public void DrawPolygon(System.Drawing.Color color, double thickness, params Point2[] points)
    {
        var pen = mapSolidPenCached(color, thickness);

        _image.Mutate(x => x.Paint(cv => { cv.DrawLine(pen, map(points)); }));
    }

    public void FillPolygon(System.Drawing.Color color, params Point2[] points)
    {
        var brush = mapSolidBrushCached(color);

        var path = new PathBuilder().AddLines(map(points)).CloseFigure().Build();

        _image.Mutate(x => x.Paint(cv => { cv.Fill(brush, path); }));
    }

    private Color mapColorCached(System.Drawing.Color color)
    {
        if (_colorMap.TryGetValue(color, out var value))
        {
            return value;
        }

        var c = Color.FromPixel(new Argb32(color.R, color.G, color.B, color.A));

        _colorMap.Add(color, c);

        return c;
    }

    private SolidPen mapSolidPenCached(System.Drawing.Color color, double thickness)
    {
        var key = new PenKey(color, thickness);

        if (_solidPensMap.TryGetValue(key, out var value))
        {
            return value;
        }

        var c = mapColorCached(color);

        var pen = new SolidPen(c, (float)thickness);

        _solidPensMap.Add(key, pen);

        return pen;
    }

    private SolidBrush mapSolidBrushCached(System.Drawing.Color color)
    {
        if (_solidBrushesMap.TryGetValue(color, out var value))
        {
            return value;
        }

        var c = mapColorCached(color);

        var pen = new SolidBrush(c);

        _solidBrushesMap.Add(color, pen);

        return pen;
    }

    private PointF map(Point2 p1) => new PointF((float)p1.X, (float)p1.Y);

    private PointF[] map(Point2[] points) => points.Select(map).ToArray();
}