namespace Cubism.Models;

public struct Size2
{
    public readonly double Width;
    public readonly double Height;

    public Size2(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }
}