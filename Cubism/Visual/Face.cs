using System.Drawing;
using Cubism.Models;

namespace Cubism.Visual;

public sealed class Face
{
    public Point3[] Vertices { get; }
    public Color Color { get; set; }
    public double Thickness { get; set; }
    public bool Stroke { get; set; } = true;

    public Face(Color color, double thickness, params Point3[] vertices)
    {
        if (vertices.Length < 3)
            throw new ArgumentException("A face needs at least three vertices.", nameof(vertices));

        Vertices = vertices;
        Color = color;
        Thickness = thickness;
    }
}
