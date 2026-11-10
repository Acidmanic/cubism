using System.Drawing;
using Cubism.Abstractions;
using Cubism.Models;

namespace Cubism.Visual;

public sealed class SceneRenderer
{
    private readonly record struct DrawCall(double Depth, Action<IRenderContext2D> Draw);

    public Projection Camera { get; }
    public Viewport Viewport { get; }
    public double Ambient { get; set; } = 0.22;
    public int TextureDivisions { get; set; } = 8;

    public SceneRenderer(Projection camera, Viewport viewport)
    {
        Camera = camera;
        Viewport = viewport;
    }

    public void DrawWireCube(Cube cube, Color color, double thickness, IRenderContext2D context)
    {
        foreach (var (a, b) in cube.Edges())
            DrawWorldSegment(a, b, color, thickness, context);
    }

    public void Render(Scene scene, IRenderContext2D context)
    {
        if (Viewport.Width <= 0 || Viewport.Height <= 0)
            return;

        var items = new List<DrawCall>();
        foreach (var root in scene.Roots)
            Collect(root, scene, items);

        items.Sort((a, b) => b.Depth.CompareTo(a.Depth));
        foreach (var item in items)
            item.Draw(context);
    }

    public bool TryPick(Scene scene, Point2 pixel, out PickHit hit)
    {
        hit = default;
        if (!Camera.TryRay(Viewport, pixel, out var origin, out var direction))
            return false;

        double best = double.PositiveInfinity;
        PickHit bestHit = default;
        bool found = false;

        foreach (var root in scene.Roots)
            Pick(root, origin, direction, ref found, ref best, ref bestHit);

        if (!found)
            return false;

        hit = bestHit;
        return true;
    }

    private void Collect(SceneObject obj, Scene scene, List<DrawCall> items)
    {
        var light = scene.Light.Direction.Normalized();

        foreach (var face in obj.Faces)
            AddFace(obj, face, light, scene.Light.Intensity, items);

        foreach (var edge in obj.Edges)
            AddEdge(obj, edge, items);

        foreach (var billboard in obj.Billboards)
            AddBillboard(obj, billboard, items);

        foreach (var child in obj.Children)
            Collect(child, scene, items);
    }

    private void AddFace(SceneObject obj, Face face, Point3 lightDirection, double intensity, List<DrawCall> items)
    {
        var local = face.Vertices;
        var world = new Point3[local.Length];
        for (int i = 0; i < local.Length; i++)
            world[i] = obj.TransformPoint(local[i]);

        if (!TryNormal(world, out var normal))
            return;

        var center = Average(world);
        if (Point3.Dot(normal, Camera.CameraPosition - center) <= 0)
            return;

        double diffuse = Math.Max(0, Point3.Dot(normal, lightDirection)) * intensity;
        double brightness = Math.Clamp(Ambient + (1 - Ambient) * diffuse, 0, 1);
        double thickness = face.Stroke ? face.Thickness : 0;
        var coords = TexCoordsFor(face);

        if (face.Fill is SolidFill)
        {
            if (!TryProjectWorld(world, out var pixels, out var depth))
                return;

            var color = Shade(face.Fill.Sample(0.5, 0.5), brightness);
            var stroke = Darken(color);
            items.Add(new DrawCall(depth, ctx =>
            {
                ctx.FillPolygon(color, pixels);
                if (thickness > 0)
                    ctx.DrawPolygon(stroke, thickness, pixels);
            }));
            return;
        }

        int divisions = Math.Max(1, TextureDivisions);
        if (local.Length == 4)
            AddTexturedQuad(obj, local, coords, face.Fill, brightness, divisions, items);
        else if (local.Length == 3)
            AddTexturedTriangle(obj, local[0], local[1], local[2], coords[0], coords[1], coords[2], face.Fill, brightness, divisions, items);
        else
        {
            for (int i = 1; i < local.Length - 1; i++)
                AddTexturedTriangle(obj, local[0], local[i], local[i + 1], coords[0], coords[i], coords[i + 1], face.Fill, brightness, divisions, items);
        }

        if (thickness > 0 && TryProjectWorld(world, out var outline, out _))
        {
            var stroke = Darken(Shade(face.Fill.Sample(0.5, 0.5), brightness));
            double depth = CloserDepth(world);
            items.Add(new DrawCall(depth, ctx => ctx.DrawPolygon(stroke, thickness, outline)));
        }
    }

    private void AddTexturedQuad(SceneObject obj, Point3[] local, Point2[] uv, FaceFill fill, double brightness, int divisions, List<DrawCall> items)
    {
        for (int y = 0; y < divisions; y++)
        {
            double t0 = y / (double)divisions;
            double t1 = (y + 1) / (double)divisions;
            for (int x = 0; x < divisions; x++)
            {
                double s0 = x / (double)divisions;
                double s1 = (x + 1) / (double)divisions;
                var cell = new[]
                {
                    Bilinear(local[0], local[1], local[2], local[3], s0, t0),
                    Bilinear(local[0], local[1], local[2], local[3], s1, t0),
                    Bilinear(local[0], local[1], local[2], local[3], s1, t1),
                    Bilinear(local[0], local[1], local[2], local[3], s0, t1)
                };
                var sample = Bilinear(uv[0], uv[1], uv[2], uv[3], (s0 + s1) / 2, (t0 + t1) / 2);
                AddLocalPolygon(obj, cell, Shade(fill.Sample(sample.X, sample.Y), brightness), items);
            }
        }
    }

    private void AddTexturedTriangle(SceneObject obj, Point3 v0, Point3 v1, Point3 v2, Point2 uv0, Point2 uv1, Point2 uv2, FaceFill fill, double brightness, int divisions, List<DrawCall> items)
    {
        double step = 1.0 / divisions;
        for (int i = 0; i < divisions; i++)
        {
            for (int j = 0; j < divisions - i; j++)
            {
                double s0 = i * step;
                double t0 = j * step;
                AddTriangleCell(obj, v0, v1, v2, uv0, uv1, uv2, s0, t0, s0 + step, t0, s0, t0 + step, fill, brightness, items);
                if (i + j < divisions - 1)
                    AddTriangleCell(obj, v0, v1, v2, uv0, uv1, uv2, s0 + step, t0, s0 + step, t0 + step, s0, t0 + step, fill, brightness, items);
            }
        }
    }

    private void AddTriangleCell(SceneObject obj, Point3 v0, Point3 v1, Point3 v2, Point2 uv0, Point2 uv1, Point2 uv2, double s0, double t0, double s1, double t1, double s2, double t2, FaceFill fill, double brightness, List<DrawCall> items)
    {
        var cell = new[]
        {
            Barycentric(v0, v1, v2, s0, t0),
            Barycentric(v0, v1, v2, s1, t1),
            Barycentric(v0, v1, v2, s2, t2)
        };
        var sample = Barycentric(uv0, uv1, uv2, (s0 + s1 + s2) / 3, (t0 + t1 + t2) / 3);
        AddLocalPolygon(obj, cell, Shade(fill.Sample(sample.X, sample.Y), brightness), items);
    }

    private void AddLocalPolygon(SceneObject obj, Point3[] local, Color color, List<DrawCall> items)
    {
        var world = new Point3[local.Length];
        for (int i = 0; i < local.Length; i++)
            world[i] = obj.TransformPoint(local[i]);

        if (!TryProjectWorld(world, out var pixels, out var depth))
            return;

        items.Add(new DrawCall(depth, ctx => ctx.FillPolygon(color, pixels)));
    }

    private bool TryProjectWorld(Point3[] world, out Point2[] pixels, out double depth)
    {
        var cameraSpace = new Point3[world.Length];
        for (int i = 0; i < world.Length; i++)
            cameraSpace[i] = Camera.ToCameraSpace(world[i]);

        var clipped = NearPlaneClip.Polygon(cameraSpace, Camera.NearPlane);
        pixels = new Point2[clipped.Count];
        depth = 0;
        if (clipped.Count < 3)
            return false;

        for (int i = 0; i < clipped.Count; i++)
        {
            depth += clipped[i].Z;
            pixels[i] = Viewport.ToPixels(Camera.ProjectCameraSpace(clipped[i]));
            if (!IsFinite(pixels[i]))
                return false;
        }

        depth /= clipped.Count;
        return true;
    }

    private double CloserDepth(Point3[] world)
    {
        double nearest = double.PositiveInfinity;
        foreach (var point in world)
        {
            double z = Camera.ToCameraSpace(point).Z;
            if (z < nearest)
                nearest = z;
        }

        return nearest - 0.001;
    }

    private static Point2[] TexCoordsFor(Face face)
    {
        if (face.TexCoords.Length == face.Vertices.Length)
            return face.TexCoords;

        return Face.DefaultTexCoords(face.Vertices.Length);
    }

    private static Point3 Bilinear(Point3 v0, Point3 v1, Point3 v2, Point3 v3, double s, double t)
    {
        double s0 = 1 - s;
        double t0 = 1 - t;
        return v0 * (s0 * t0) + v1 * (s * t0) + v2 * (s * t) + v3 * (s0 * t);
    }

    private static Point2 Bilinear(Point2 v0, Point2 v1, Point2 v2, Point2 v3, double s, double t)
    {
        double s0 = 1 - s;
        double t0 = 1 - t;
        return new Point2(
            v0.X * (s0 * t0) + v1.X * (s * t0) + v2.X * (s * t) + v3.X * (s0 * t),
            v0.Y * (s0 * t0) + v1.Y * (s * t0) + v2.Y * (s * t) + v3.Y * (s0 * t));
    }

    private static Point3 Barycentric(Point3 v0, Point3 v1, Point3 v2, double s, double t)
        => v0 * (1 - s - t) + v1 * s + v2 * t;

    private static Point2 Barycentric(Point2 v0, Point2 v1, Point2 v2, double s, double t)
        => new(
            v0.X * (1 - s - t) + v1.X * s + v2.X * t,
            v0.Y * (1 - s - t) + v1.Y * s + v2.Y * t);

    private void AddEdge(SceneObject obj, Edge edge, List<DrawCall> items)
    {
        var a = Camera.ToCameraSpace(obj.TransformPoint(edge.A));
        var b = Camera.ToCameraSpace(obj.TransformPoint(edge.B));
        if (!NearPlaneClip.Segment(a, b, Camera.NearPlane, out var clippedA, out var clippedB))
            return;

        var p1 = Viewport.ToPixels(Camera.ProjectCameraSpace(clippedA));
        var p2 = Viewport.ToPixels(Camera.ProjectCameraSpace(clippedB));
        if (!IsFinite(p1) || !IsFinite(p2) || edge.Thickness <= 0)
            return;

        var color = edge.Color;
        double thickness = edge.Thickness;
        double depth = (clippedA.Z + clippedB.Z) / 2;
        items.Add(new DrawCall(depth, ctx => ctx.DrawLine(color, thickness, p1, p2)));
    }

    private void AddBillboard(SceneObject obj, Billboard billboard, List<DrawCall> items)
    {
        if (billboard.Width <= 0 || billboard.Height <= 0)
            return;

        var camera = Camera.ToCameraSpace(obj.TransformPoint(billboard.LocalCenter));
        if (camera.Z <= Camera.NearPlane)
            return;

        var center = Viewport.ToPixels(Camera.ProjectCameraSpace(camera));
        if (!IsFinite(center))
            return;

        double pixelsPerUnit = Camera.CameraPointOfView.FocalLength / camera.Z * Viewport.Height;
        double pixelWidth = billboard.Width * pixelsPerUnit;
        double pixelHeight = billboard.Height * pixelsPerUnit;
        var topLeft = new Point2(center.X - pixelWidth / 2, center.Y - pixelHeight / 2);
        var image = billboard.Image;
        items.Add(new DrawCall(camera.Z, ctx => ctx.DrawImage(image, topLeft, pixelWidth, pixelHeight)));
    }

    private void DrawWorldSegment(Point3 worldA, Point3 worldB, Color color, double thickness, IRenderContext2D context)
    {
        if (thickness <= 0)
            return;

        var a = Camera.ToCameraSpace(worldA);
        var b = Camera.ToCameraSpace(worldB);
        if (!NearPlaneClip.Segment(a, b, Camera.NearPlane, out var clippedA, out var clippedB))
            return;

        var p1 = Viewport.ToPixels(Camera.ProjectCameraSpace(clippedA));
        var p2 = Viewport.ToPixels(Camera.ProjectCameraSpace(clippedB));
        if (!IsFinite(p1) || !IsFinite(p2))
            return;

        context.DrawLine(color, thickness, p1, p2);
    }

    private void Pick(SceneObject obj, Point3 origin, Point3 direction, ref bool found, ref double best, ref PickHit bestHit)
    {
        for (int index = 0; index < obj.Faces.Count; index++)
        {
            var vertices = obj.Faces[index].Vertices;
            var world = new Point3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                world[i] = obj.TransformPoint(vertices[i]);

            if (!TryHitFace(world, origin, direction, out var point, out var distance) || distance >= best)
                continue;

            if (Camera.ToCameraSpace(point).Z <= Camera.NearPlane)
                continue;

            best = distance;
            found = true;
            bestHit = new PickHit(obj, index, point, distance);
        }

        foreach (var child in obj.Children)
            Pick(child, origin, direction, ref found, ref best, ref bestHit);
    }

    private static bool TryHitFace(Point3[] world, Point3 origin, Point3 direction, out Point3 point, out double distance)
    {
        point = default;
        distance = 0;

        if (!TryNormal(world, out var normal))
            return false;

        double denom = Point3.Dot(normal, direction);
        if (denom >= -1e-8)
            return false;

        double t = Point3.Dot(normal, world[0] - origin) / denom;
        if (t < 0)
            return false;

        point = origin + direction * t;
        if (!Contains(point, world, normal))
            return false;

        distance = t;
        return true;
    }

    private static bool Contains(Point3 point, Point3[] polygon, Point3 normal)
    {
        for (int i = 0; i < polygon.Length; i++)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % polygon.Length];
            var cross = Point3.Cross(b - a, point - a);
            if (Point3.Dot(cross, normal) < -1e-4)
                return false;
        }

        return true;
    }

    private static bool TryNormal(Point3[] world, out Point3 normal)
    {
        normal = default;
        if (world.Length < 3)
            return false;

        normal = Point3.Cross(world[1] - world[0], world[2] - world[0]);
        if (normal.Length <= 1e-8)
            return false;

        normal = normal.Normalized();
        return true;
    }

    private static Point3 Average(Point3[] points)
    {
        double x = 0, y = 0, z = 0;
        foreach (var point in points)
        {
            x += point.X;
            y += point.Y;
            z += point.Z;
        }

        double n = points.Length;
        return new Point3(x / n, y / n, z / n);
    }

    private static Color Shade(Color color, double brightness)
    {
        brightness = Math.Clamp(brightness, 0, 1);
        return Color.FromArgb(
            color.A,
            (int)Math.Round(color.R * brightness),
            (int)Math.Round(color.G * brightness),
            (int)Math.Round(color.B * brightness));
    }

    private static Color Darken(Color color)
    {
        return Color.FromArgb(color.A, Scale(color.R), Scale(color.G), Scale(color.B));
    }

    private static int Scale(byte channel) => (int)Math.Clamp(channel * 0.45, 0, 255);

    private static bool IsFinite(Point2 point) => double.IsFinite(point.X) && double.IsFinite(point.Y);
}
