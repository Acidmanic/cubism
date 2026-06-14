namespace Cubism.Models;

public struct Point2
{
    public readonly double X;
    public readonly double Y;

    public Point2(double x, double y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }


    public Point2Absolute ToAbsolute(Size3Absolute size)
    {
        return new Point2Absolute((int)Math.Floor(X * size.Width), (int)Math.Floor(Y * size.Height));
    }

    public Point2 Bound(Size2 size)
    {
        var x = X;
        var y = Y;

        if (x < 0) x = 0;
        if (y < 0) y = 0;

        if (x > size.Width) x = size.Width;
        if (y > size.Height) y = size.Height;

        return new Point2(x, y);
    }
}