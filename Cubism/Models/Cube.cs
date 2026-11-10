namespace Cubism.Models;

public class Cube:VectorObject
{
    public Point3 TopLeftBack { get; private set; }
    public Point3 BottomRightBack { get; private set; }
    public Point3 TopRightBack { get; private set; }
    public Point3 BottomLeftBack { get; private set; }

    public Point3 TopLeftFront { get; private set; }
    public Point3 BottomRightFront { get; private set; }
    public Point3 TopRightFront { get; private set; }
    public Point3 BottomLeftFront { get; private set; }

    public double Width { get; private set; }
    public double Height { get; private set; }
    public double Depth { get; private set; }

    public Size3 Size { get; private set; }

    public Cube(double x, double y, double z, double width, double height, double depth)
    {
        TopLeftBack = new Point3(x, y, z);
        BottomRightBack = new Point3(x + width, y, z);
        TopRightBack = new Point3(x + width, y + height, z);
        BottomLeftBack = new Point3(x + width, y + height, z);
        TopLeftFront = new Point3(x, y, z + depth);
        BottomRightFront = new Point3(x + width, y + height, z + depth);
        TopRightFront = new Point3(x + width, y + height, z + depth);
        BottomLeftFront = new Point3(x + width, y + height, z + depth);
        Size =  new Size3(width, height, depth);
        Width = width;
        Height = height;
        Depth = depth;
    }

    public static bool operator ==(Cube c1, Cube c2)
    {
        return c1.TopLeftBack == c2.TopLeftBack;
    }

    public static bool operator !=(Cube cube1, Cube cube2)
    {
        return !(cube1 == cube2);
    }
}