using System.Drawing;

namespace Cubism.Visual;

public sealed class Bitmap2D
{
    public int Width { get; }
    public int Height { get; }
    public byte[] Rgba { get; }

    public Bitmap2D(int width, int height, byte[] rgba)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (rgba.Length != width * height * 4)
            throw new ArgumentException("RGBA buffer length must be width * height * 4.", nameof(rgba));

        Width = width;
        Height = height;
        Rgba = rgba;
    }

    public static Bitmap2D Solid(int width, int height, Color color)
    {
        var rgba = new byte[width * height * 4];
        for (int i = 0; i < rgba.Length; i += 4)
        {
            rgba[i] = color.R;
            rgba[i + 1] = color.G;
            rgba[i + 2] = color.B;
            rgba[i + 3] = color.A;
        }

        return new Bitmap2D(width, height, rgba);
    }

    public static Bitmap2D Checker(int width, int height, Color a, Color b, int cell)
    {
        if (cell <= 0) throw new ArgumentOutOfRangeException(nameof(cell));

        var rgba = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = ((x / cell) + (y / cell)) % 2 == 0 ? a : b;
                int i = (y * width + x) * 4;
                rgba[i] = color.R;
                rgba[i + 1] = color.G;
                rgba[i + 2] = color.B;
                rgba[i + 3] = color.A;
            }
        }

        return new Bitmap2D(width, height, rgba);
    }
}
