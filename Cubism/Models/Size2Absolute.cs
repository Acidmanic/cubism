namespace Cubism.Models;

public struct Size2Absolute
{
    public readonly int Width;
    public readonly int Height;

    public Size2Absolute(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }
}