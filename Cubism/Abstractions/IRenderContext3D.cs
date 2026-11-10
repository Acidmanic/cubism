using System.Drawing;
using Cubism.Models;

namespace Cubism.Abstractions;

public interface IRenderContext3D
{
    void DrawOrthogonalCube(Color color,double thickness, Point3 topLeftBackCorner, double width, double height, double depth);
}