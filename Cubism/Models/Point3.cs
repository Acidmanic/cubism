namespace Cubism.Models;

public struct Point3
{
    public readonly double X;
    public readonly double Y;
    public readonly double Z;

    public Point3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

    public Point3 Normalized()
    {
        double length = Length;
        if (length <= 1e-12)
            return new Point3(0, 0, 0);

        return new Point3(X / length, Y / length, Z / length);
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public Point3Absolute ToAbsolute(Size3Absolute size)
    {
        return new Point3Absolute((int)Math.Floor(X * size.Width), (int)Math.Floor(Y * size.Height), (int)Math.Floor(Z * size.Depth));
    }

    public Point3 Bound(Size3 size)
    {
        var x = X;
        var y = Y;
        var z = Z;

        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (z < 0) z = 0;

        if (x > size.Width) x = size.Width;
        if (y > size.Height) y = size.Height;
        if (z > size.Depth) z = size.Depth;

        return new Point3(x, y, z);
    }

    public static Point3 operator +(Point3 a, Point3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Point3 operator -(Point3 a, Point3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Point3 operator *(Point3 a, double scale) => new(a.X * scale, a.Y * scale, a.Z * scale);

    public static double Dot(Point3 a, Point3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Point3 Cross(Point3 a, Point3 b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public static bool operator ==(Point3 a, Point3 b)
    {
        return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    }

    public static bool operator !=(Point3 a, Point3 b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj) => obj is Point3 other && this == other;

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
}
