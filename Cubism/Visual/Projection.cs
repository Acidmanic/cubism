using Cubism.Models;

namespace Cubism.Visual;

public class Projection
{
    public Point3 CameraPosition { get; set; } = new Point3(0.5, 0.5, 1);

    public PointOfView CameraPointOfView { get; set; } = PointOfView.DefaultCameraPointOfView;


    public Point2 Project(Point3 point)
    {
        // 1. Translate relative to camera
        double x = point.X - CameraPosition.X;
        double y = point.Y - CameraPosition.Y;
        double z = point.Z - CameraPosition.Z;

        // 2. Yaw rotation (around Y axis)
        double cosy = Math.Cos(CameraPointOfView.Yaw);
        double siny = Math.Sin(CameraPointOfView.Yaw);

        double x1 = x * cosy - z * siny;
        double z1 = x * siny + z * cosy;

        // 3. Pitch rotation (around X axis)
        double cosp = Math.Cos(CameraPointOfView.Pitch);
        double sinp = Math.Sin(CameraPointOfView.Pitch);

        double y2 = y * cosp - z1 * sinp;
        double z2 = y * sinp + z1 * cosp;

        // Avoid division by zero / points behind camera
        if (z2 <= 0.0001)
            return new Point2(double.NaN, double.NaN);

        // 4. Perspective projection
        double px = (x1 * CameraPointOfView.FocalLength) / z2;
        double py = (y2 * CameraPointOfView.FocalLength) / z2;

        // 5. Convert to normalized [0,1] screen space
        // Center is (0.5, 0.5)
        double screenX = 0.5 + px;
        double screenY = 0.5 - py;

        return new Point2(screenX, screenY);
    }
}