using System.Drawing;

namespace Cubism.Visual;

public abstract class FaceFill
{
    public abstract Color Sample(double u, double v);
}

public sealed class SolidFill : FaceFill
{
    public Color Color { get; }

    public SolidFill(Color color)
    {
        Color = color;
    }

    public override Color Sample(double u, double v) => Color;
}

public sealed class ImageFill : FaceFill
{
    public Bitmap2D Image { get; }

    public ImageFill(Bitmap2D image)
    {
        Image = image;
    }

    public override Color Sample(double u, double v)
    {
        u = Math.Clamp(u, 0, 1);
        v = Math.Clamp(v, 0, 1);
        int x = (int)Math.Clamp(u * (Image.Width - 1), 0, Image.Width - 1);
        int y = (int)Math.Clamp((1 - v) * (Image.Height - 1), 0, Image.Height - 1);
        int i = (y * Image.Width + x) * 4;
        return Color.FromArgb(Image.Rgba[i + 3], Image.Rgba[i], Image.Rgba[i + 1], Image.Rgba[i + 2]);
    }
}

public sealed class GradientFill : FaceFill
{
    public Color From { get; }
    public Color To { get; }
    public bool AlongV { get; }

    public GradientFill(Color from, Color to, bool alongV = false)
    {
        From = from;
        To = to;
        AlongV = alongV;
    }

    public override Color Sample(double u, double v)
    {
        double t = Math.Clamp(AlongV ? v : u, 0, 1);
        return Color.FromArgb(
            Blend(From.A, To.A, t),
            Blend(From.R, To.R, t),
            Blend(From.G, To.G, t),
            Blend(From.B, To.B, t));
    }

    private static int Blend(byte from, byte to, double t) => (int)Math.Round(from + (to - from) * t);
}
