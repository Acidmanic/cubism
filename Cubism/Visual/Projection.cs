using Cubism.Models;

namespace Cubism.Visual;

/// <summary>
/// Perspective camera.
/// World units are arbitrary doubles, Y is up, and the camera looks along +Z after yaw and pitch.
/// <see cref="Project"/> returns normalized screen space, center at (0.5, 0.5), Y up.
/// <see cref="Viewport"/> is what turns that into pixels and corrects aspect ratio.
/// A point at or behind <see cref="NearPlane"/> projects to NaN; segment clipping lives in the renderer.
/// </summary>
public class Projection
{
    public const double DefaultNearPlane = 0.0001;

    public Point3 CameraPosition { get; set; } = new Point3(0.5, 0.5, 1);

    public PointOfView CameraPointOfView { get; set; } = PointOfView.DefaultCameraPointOfView;

    public double NearPlane { get; set; } = DefaultNearPlane;

    public void LookAt(Point3 target)
    {
        double dx = target.X - CameraPosition.X;
        double dy = target.Y - CameraPosition.Y;
        double dz = target.Z - CameraPosition.Z;
        double yaw = Math.Atan2(dx, dz);
        double pitch = Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz));
        CameraPointOfView = new PointOfView(yaw, pitch, CameraPointOfView.FocalLength);
    }

    public Point3 ToCameraSpace(Point3 point)
    {
        double x = point.X - CameraPosition.X;
        double y = point.Y - CameraPosition.Y;
        double z = point.Z - CameraPosition.Z;

        double cosy = Math.Cos(CameraPointOfView.Yaw);
        double siny = Math.Sin(CameraPointOfView.Yaw);
        double x1 = x * cosy - z * siny;
        double z1 = x * siny + z * cosy;

        double cosp = Math.Cos(CameraPointOfView.Pitch);
        double sinp = Math.Sin(CameraPointOfView.Pitch);
        double y2 = y * cosp - z1 * sinp;
        double z2 = y * sinp + z1 * cosp;

        return new Point3(x1, y2, z2);
    }

    public Point3 CameraDirectionToWorld(Point3 cameraDirection)
    {
        double cosy = Math.Cos(CameraPointOfView.Yaw);
        double siny = Math.Sin(CameraPointOfView.Yaw);
        double cosp = Math.Cos(CameraPointOfView.Pitch);
        double sinp = Math.Sin(CameraPointOfView.Pitch);

        double y = cameraDirection.Y * cosp + cameraDirection.Z * sinp;
        double z1 = -cameraDirection.Y * sinp + cameraDirection.Z * cosp;
        double x = cameraDirection.X * cosy + z1 * siny;
        double z = -cameraDirection.X * siny + z1 * cosy;

        return new Point3(x, y, z);
    }

    public Point2 Project(Point3 point)
    {
        var camera = ToCameraSpace(point);
        if (camera.Z <= NearPlane)
            return new Point2(double.NaN, double.NaN);

        return ProjectCameraSpace(camera);
    }

    public Point2 ProjectCameraSpace(Point3 cameraSpace)
    {
        double px = (cameraSpace.X * CameraPointOfView.FocalLength) / cameraSpace.Z;
        double py = (cameraSpace.Y * CameraPointOfView.FocalLength) / cameraSpace.Z;
        return new Point2(0.5 + px, 0.5 - py);
    }

    public bool TryRay(Viewport viewport, Point2 pixel, out Point3 origin, out Point3 direction)
    {
        origin = CameraPosition;
        direction = default;

        if (viewport.Height <= 0 || CameraPointOfView.FocalLength == 0)
            return false;

        double px = (pixel.X - viewport.Width / 2) / viewport.Height;
        double py = 0.5 - (pixel.Y / viewport.Height);
        double focal = CameraPointOfView.FocalLength;
        var world = CameraDirectionToWorld(new Point3(px / focal, py / focal, 1));
        if (world.Length <= 1e-12)
            return false;

        direction = world.Normalized();
        return true;
    }
}
