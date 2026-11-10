namespace Cubism.Models;

public class Cube
{
    public Point3 BottomLeftBack { get; }
    public Point3 BottomRightBack { get; }
    public Point3 TopLeftBack { get; }
    public Point3 TopRightBack { get; }

    public Point3 BottomLeftFront { get; }
    public Point3 BottomRightFront { get; }
    public Point3 TopLeftFront { get; }
    public Point3 TopRightFront { get; }

    public double Width { get; }
    public double Height { get; }
    public double Depth { get; }

    public Size3 Size { get; }

    public Cube(double x, double y, double z, double width, double height, double depth)
    {
        if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (depth < 0) throw new ArgumentOutOfRangeException(nameof(depth));

        Width = width;
        Height = height;
        Depth = depth;
        Size = new Size3(width, height, depth);

        BottomLeftBack = new Point3(x, y, z);
        BottomRightBack = new Point3(x + width, y, z);
        TopLeftBack = new Point3(x, y + height, z);
        TopRightBack = new Point3(x + width, y + height, z);

        BottomLeftFront = new Point3(x, y, z + depth);
        BottomRightFront = new Point3(x + width, y, z + depth);
        TopLeftFront = new Point3(x, y + height, z + depth);
        TopRightFront = new Point3(x + width, y + height, z + depth);
    }

    public IReadOnlyList<Point3> Corners =>
    [
        BottomLeftBack, BottomRightBack, TopLeftBack, TopRightBack,
        BottomLeftFront, BottomRightFront, TopLeftFront, TopRightFront
    ];

    public IEnumerable<(Point3 A, Point3 B)> Edges()
    {
        yield return (BottomLeftBack, BottomRightBack);
        yield return (BottomRightBack, TopRightBack);
        yield return (TopRightBack, TopLeftBack);
        yield return (TopLeftBack, BottomLeftBack);

        yield return (BottomLeftFront, BottomRightFront);
        yield return (BottomRightFront, TopRightFront);
        yield return (TopRightFront, TopLeftFront);
        yield return (TopLeftFront, BottomLeftFront);

        yield return (BottomLeftBack, BottomLeftFront);
        yield return (BottomRightBack, BottomRightFront);
        yield return (TopLeftBack, TopLeftFront);
        yield return (TopRightBack, TopRightFront);
    }

    public IEnumerable<Point3[]> Faces()
    {
        yield return [BottomLeftFront, BottomRightFront, TopRightFront, TopLeftFront];
        yield return [BottomRightBack, BottomLeftBack, TopLeftBack, TopRightBack];
        yield return [BottomLeftBack, BottomLeftFront, TopLeftFront, TopLeftBack];
        yield return [BottomRightBack, TopRightBack, TopRightFront, BottomRightFront];
        yield return [TopLeftBack, TopLeftFront, TopRightFront, TopRightBack];
        yield return [BottomLeftBack, BottomRightBack, BottomRightFront, BottomLeftFront];
    }

    public bool Overlaps(Cube other)
    {
        double ax = BottomLeftBack.X;
        double ay = BottomLeftBack.Y;
        double az = BottomLeftBack.Z;
        double bx = other.BottomLeftBack.X;
        double by = other.BottomLeftBack.Y;
        double bz = other.BottomLeftBack.Z;

        return ax <= bx + other.Width && bx <= ax + Width
            && ay <= by + other.Height && by <= ay + Height
            && az <= bz + other.Depth && bz <= az + Depth;
    }

    public static bool operator ==(Cube? cube1, Cube? cube2)
    {
        if (ReferenceEquals(cube1, cube2)) return true;
        if (cube1 is null || cube2 is null) return false;

        return cube1.BottomLeftBack == cube2.BottomLeftBack
            && cube1.Width == cube2.Width
            && cube1.Height == cube2.Height
            && cube1.Depth == cube2.Depth;
    }

    public static bool operator !=(Cube? cube1, Cube? cube2)
    {
        return !(cube1 == cube2);
    }

    public override bool Equals(object? obj) => obj is Cube other && this == other;

    public override int GetHashCode() => HashCode.Combine(BottomLeftBack, Width, Height, Depth);
}
