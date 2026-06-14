namespace Cubism.Models;

public struct Point2Absolute
{
    public readonly int X;
    public readonly int Y;

    public Point2Absolute(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}