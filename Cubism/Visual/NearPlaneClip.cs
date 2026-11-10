using Cubism.Models;

namespace Cubism.Visual;

public static class NearPlaneClip
{
    public static List<Point3> Polygon(IReadOnlyList<Point3> cameraSpace, double near)
    {
        var output = new List<Point3>();
        if (cameraSpace.Count == 0)
            return output;

        var previous = cameraSpace[^1];
        bool previousInside = previous.Z >= near;

        for (int i = 0; i < cameraSpace.Count; i++)
        {
            var current = cameraSpace[i];
            bool currentInside = current.Z >= near;

            if (currentInside != previousInside)
                output.Add(Intersect(previous, current, near));

            if (currentInside)
                output.Add(current);

            previous = current;
            previousInside = currentInside;
        }

        return output;
    }

    public static bool Segment(Point3 a, Point3 b, double near, out Point3 clippedA, out Point3 clippedB)
    {
        bool aInside = a.Z >= near;
        bool bInside = b.Z >= near;
        clippedA = a;
        clippedB = b;

        if (!aInside && !bInside)
            return false;

        if (aInside && bInside)
            return true;

        var hit = Intersect(a, b, near);
        if (aInside)
            clippedB = hit;
        else
            clippedA = hit;

        return true;
    }

    private static Point3 Intersect(Point3 a, Point3 b, double near)
    {
        double dz = b.Z - a.Z;
        double t = dz == 0 ? 0 : (near - a.Z) / dz;
        return new Point3(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            near);
    }
}
