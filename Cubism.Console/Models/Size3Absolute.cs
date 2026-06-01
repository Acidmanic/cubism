namespace Cubism.Console.Models;

public struct Size3Absolute
{
    public int Width { get;  }
    public int Height { get;  }
    public int Depth { get;  }

    public Size3Absolute(int width, int height, int depth)
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