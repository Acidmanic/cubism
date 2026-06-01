namespace Cubism.Console.Models;

public struct Point3Absolute
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    
    public Point3Absolute(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

}