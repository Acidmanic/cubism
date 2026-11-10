using System.Drawing;
using Cubism.Models;

namespace Cubism.Visual;

public readonly struct Edge
{
    public Point3 A { get; }
    public Point3 B { get; }
    public Color Color { get; }
    public double Thickness { get; }

    public Edge(Point3 a, Point3 b, Color color, double thickness)
    {
        A = a;
        B = b;
        Color = color;
        Thickness = thickness;
    }
}
