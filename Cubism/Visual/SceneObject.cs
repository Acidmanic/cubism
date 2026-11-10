using System.Drawing;
using System.Numerics;
using Cubism.Models;

namespace Cubism.Visual;

public sealed class SceneObject
{
    private readonly List<SceneObject> _children = new();

    public Point3 Position { get; set; }
    public double Yaw { get; set; }
    public double Pitch { get; set; }
    public double Roll { get; set; }
    public Point3 Scale { get; set; } = new(1, 1, 1);

    public SceneObject? Parent { get; private set; }
    public IReadOnlyList<SceneObject> Children => _children;
    public List<Face> Faces { get; } = new();
    public List<Edge> Edges { get; } = new();
    public List<Billboard> Billboards { get; } = new();

    public void AddChild(SceneObject child)
    {
        if (child == this)
            throw new InvalidOperationException("An object cannot parent itself.");

        for (var ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ancestor == child)
                throw new InvalidOperationException("Adding this child would cycle the scene graph.");
        }

        child.Detach();
        child.Parent = this;
        _children.Add(child);
    }

    public void Detach()
    {
        Parent?._children.Remove(this);
        Parent = null;
    }

    public Matrix4x4 WorldMatrix()
    {
        var local = Matrix4x4.CreateScale((float)Scale.X, (float)Scale.Y, (float)Scale.Z)
            * Matrix4x4.CreateRotationZ((float)Roll)
            * Matrix4x4.CreateRotationX((float)Pitch)
            * Matrix4x4.CreateRotationY((float)Yaw)
            * Matrix4x4.CreateTranslation((float)Position.X, (float)Position.Y, (float)Position.Z);

        return Parent is null ? local : local * Parent.WorldMatrix();
    }

    public Point3 TransformPoint(Point3 local)
    {
        var transformed = Vector3.Transform(
            new Vector3((float)local.X, (float)local.Y, (float)local.Z),
            WorldMatrix());

        return new Point3(transformed.X, transformed.Y, transformed.Z);
    }

    public static SceneObject Cube(double x, double y, double z, double width, double height, double depth, Color color, double thickness = 1.5)
    {
        var cube = new Cube(0, 0, 0, width, height, depth);
        var obj = new SceneObject { Position = new Point3(x, y, z) };

        foreach (var face in cube.Faces())
            obj.Faces.Add(new Face(color, thickness, face));

        return obj;
    }
}
