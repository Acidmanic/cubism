using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cubism.Abstractions;
using Cubism.Models;
using Cubism.Visual;
using DrawingContext = global::Avalonia.Media.DrawingContext;
using Pen = global::Avalonia.Media.Pen;
using SolidColorBrush = global::Avalonia.Media.SolidColorBrush;
using StreamGeometry = global::Avalonia.Media.StreamGeometry;
using Bitmap = global::Avalonia.Media.Imaging.Bitmap;
using WriteableBitmap = global::Avalonia.Media.Imaging.WriteableBitmap;
using AlphaFormat = global::Avalonia.Platform.AlphaFormat;
using PixelFormat = global::Avalonia.Platform.PixelFormat;

namespace Cubism.Avalonia;

public sealed class AvaloniaRenderContext2D : IRenderContext2D
{
    private static readonly ConditionalWeakTable<Bitmap2D, Bitmap> Bitmaps = new();

    private readonly DrawingContext _context;
    private readonly Dictionary<int, SolidColorBrush> _brushes = new();
    private readonly Dictionary<(int Argb, double Thickness), Pen> _pens = new();

    public AvaloniaRenderContext2D(DrawingContext context)
    {
        _context = context;
    }

    public void DrawLine(System.Drawing.Color color, double thickness, Point2 p1, Point2 p2)
    {
        if (thickness <= 0)
            return;

        _context.DrawLine(PenFor(color, thickness), Map(p1), Map(p2));
    }

    public void DrawPolygon(System.Drawing.Color color, double thickness, params Point2[] points)
    {
        if (points.Length < 2 || thickness <= 0)
            return;

        _context.DrawGeometry(null, PenFor(color, thickness), Geometry(points, false));
    }

    public void FillPolygon(System.Drawing.Color color, params Point2[] points)
    {
        if (points.Length < 3)
            return;

        _context.DrawGeometry(BrushFor(color), null, Geometry(points, true));
    }

    public void DrawImage(Bitmap2D image, Point2 topLeft, double width, double height)
    {
        if (width < 1 || height < 1)
            return;

        _context.DrawImage(BitmapFor(image), new global::Avalonia.Rect(topLeft.X, topLeft.Y, width, height));
    }

    private static Bitmap BitmapFor(Bitmap2D image)
    {
        if (Bitmaps.TryGetValue(image, out var existing))
            return existing;

        var bitmap = new WriteableBitmap(
            new global::Avalonia.PixelSize(image.Width, image.Height),
            new global::Avalonia.Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul);

        using (var buffer = bitmap.Lock())
        {
            int rowBytes = image.Width * 4;
            for (int y = 0; y < image.Height; y++)
                Marshal.Copy(image.Rgba, y * rowBytes, buffer.Address + y * buffer.RowBytes, rowBytes);
        }

        Bitmaps.Add(image, bitmap);
        return bitmap;
    }

    private SolidColorBrush BrushFor(System.Drawing.Color color)
    {
        if (_brushes.TryGetValue(color.ToArgb(), out var brush))
            return brush;

        brush = new SolidColorBrush(ToAvalonia(color));
        _brushes.Add(color.ToArgb(), brush);
        return brush;
    }

    private Pen PenFor(System.Drawing.Color color, double thickness)
    {
        var key = (color.ToArgb(), thickness);
        if (_pens.TryGetValue(key, out var pen))
            return pen;

        pen = new Pen { Brush = BrushFor(color), Thickness = thickness };
        _pens.Add(key, pen);
        return pen;
    }

    private static StreamGeometry Geometry(Point2[] points, bool filled)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(Map(points[0]), filled);
            for (int i = 1; i < points.Length; i++)
                context.LineTo(Map(points[i]));
            context.EndFigure(true);
        }

        return geometry;
    }

    private static global::Avalonia.Point Map(Point2 point) => new(point.X, point.Y);

    private static global::Avalonia.Media.Color ToAvalonia(System.Drawing.Color color)
        => global::Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
}
