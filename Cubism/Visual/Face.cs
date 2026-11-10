using System.Drawing;
using Cubism.Models;

namespace Cubism.Visual;

public sealed class Face
{
    public Point3[] Vertices { get; }
    public Point2[] TexCoords { get; set; }
    public FaceFill Fill { get; set; }
    public double Thickness { get; set; }
    public bool Stroke { get; set; } = true;

    public Color Color
    {
        get => Fill.Sample(0.5, 0.5);
        set => Fill = new SolidFill(value);
    }

    public Face(Color color, double thickness, params Point3[] vertices)
        : this(new SolidFill(color), thickness, vertices)
    {
    }

    public Face(FaceFill fill, double thickness, params Point3[] vertices)
    {
        if (vertices.Length < 3)
            throw new ArgumentException("A face needs at least three vertices.", nameof(vertices));

        Vertices = vertices;
        Fill = fill;
        Thickness = thickness;
        TexCoords = DefaultTexCoords(vertices.Length);
    }

    public static Point2[] DefaultTexCoords(int count)
    {
        if (count == 4)
            return [new Point2(0, 0), new Point2(1, 0), new Point2(1, 1), new Point2(0, 1)];

        if (count == 3)
            return [new Point2(0, 0), new Point2(1, 0), new Point2(0, 1)];

        var coords = new Point2[count];
        for (int i = 0; i < count; i++)
        {
            double angle = i / (double)count * Math.PI * 2;
            coords[i] = new Point2(0.5 + 0.5 * Math.Cos(angle), 0.5 + 0.5 * Math.Sin(angle));
        }

        return coords;
    }
}
