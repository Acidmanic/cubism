namespace Cubism.Models;

public struct Size3
{
    public double Width { get;  }
    public double Height { get;  }
    public double Depth { get;  }

    public Size3(double width, double height, double depth)
    {
        Width = width;
        Height = height;
        Depth = depth;
    }

    public override string ToString()
    {
        return $"{Width}x{Height}x{Depth}";
    }
}