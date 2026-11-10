using System.Drawing;
using Cubism.Models;
using Cubism.Visual;

namespace Cubism.Abstractions;

public interface IRenderContext2D
{
    void DrawLine(Color color, double thickness, Point2 p1, Point2 p2);

    void DrawPolygon(Color color, double thickness, params Point2[] points);

    void FillPolygon(Color color, params Point2[] points);

    void DrawImage(Bitmap2D image, Point2 topLeft, double width, double height);
}