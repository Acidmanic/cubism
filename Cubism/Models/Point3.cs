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
        var  y = Y;
        var  z = Z;

        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (z < 0) z = 0;
        
        if(x > size.Width) x = size.Width;
        if (y > size.Height) y = size.Height;
        if (z > size.Depth) z = size.Depth;
        
        return new Point3(x, y, z);
    }
}