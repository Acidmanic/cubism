using Cubism.Models;

namespace Cubism.Visual;

public readonly struct PickHit
{
    public SceneObject Object { get; }
    public int FaceIndex { get; }
    public Point3 Point { get; }
    public double Distance { get; }

    public PickHit(SceneObject obj, int faceIndex, Point3 point, double distance)
    {
        Object = obj;
        FaceIndex = faceIndex;
        Point = point;
        Distance = distance;
    }
}
