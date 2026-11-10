using Cubism.Models;

namespace Cubism.Visual;

public sealed class Billboard
{
    public Point3 LocalCenter { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public Bitmap2D Image { get; set; }

    public Billboard(Point3 localCenter, double width, double height, Bitmap2D image)
    {
        LocalCenter = localCenter;
        Width = width;
        Height = height;
        Image = image;
    }
}
