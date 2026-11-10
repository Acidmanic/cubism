using Cubism.Models;

namespace Cubism.Visual;

public readonly struct Viewport
{
    public double Width { get; }
    public double Height { get; }

    public Viewport(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public Point2 ToPixels(Point2 normalized)
    {
        double x = Width / 2 + (normalized.X - 0.5) * Height;
        double y = normalized.Y * Height;
        return new Point2(x, y);
    }
}
