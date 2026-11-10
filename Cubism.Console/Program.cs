using System.Drawing;
using Color = System.Drawing.Color;
using Cubism.Extensions;
using Cubism.ImageSharp;
using Cubism.Models;
using Cubism.Visual;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

const int width = 800;
const int height = 600;
var background = Color.FromArgb(255, 24, 26, 32);

var straight = new Projection { CameraPosition = new Point3(0.5, 0.5, -2) };
straight.LookAt(new Point3(0.5, 0.5, 0.5));
var viewport = new Viewport(width, height);

VerifyGeometry();
VerifyViewport(viewport);
VerifyProjection(straight);
VerifyClip();
VerifyHierarchy();
VerifyOverlap();
VerifyPick(straight, viewport);

var camera = new Projection { CameraPosition = new Point3(2.2, 1.5, -2.6) };
var lookTarget = new Point3(0.7, 0.55, 0.4);
camera.LookAt(lookTarget);
var aimed = camera.ToCameraSpace(lookTarget);
if (Math.Abs(aimed.X) > 1e-6 || Math.Abs(aimed.Y) > 1e-6 || aimed.Z <= 0)
    throw new InvalidOperationException($"LookAt missed the target: {aimed}.");

var scene = BuildScene();
using var image = new Image<Rgba32>(width, height);
var context = new ImageSharpRenderContext2D(image);
context.FillRectangle(background, new Point2(0, 0), width, height);
new SceneRenderer(camera, viewport).Render(scene, context);

int shaded = CountDiffering(image, background);
int checker = CountColor(image, Color.FromArgb(255, 230, 40, 140));
if (shaded < 1000)
    throw new InvalidOperationException($"Filled scene drew only {shaded} pixels.");
if (checker < 20)
    throw new InvalidOperationException($"Billboard checker drew only {checker} pixels.");

var path = Path.GetFullPath("cube.png");
image.Save(path);

using var wire = new Image<Rgba32>(width, height);
var wireContext = new ImageSharpRenderContext2D(wire);
wireContext.FillRectangle(background, new Point2(0, 0), width, height);
new SceneRenderer(straight, viewport).DrawWireCube(new Cube(0, 0, 0, 1, 1, 1), Color.White, 2, wireContext);
int wirePixels = CountDiffering(wire, background);
if (wirePixels < 200)
    throw new InvalidOperationException($"Wire cube drew only {wirePixels} pixels.");

Console.WriteLine($"Wrote {path} ({shaded} shaded pixels, {checker} billboard pixels, {wirePixels} wire pixels).");

static Scene BuildScene()
{
    var scene = new Scene();
    var body = SceneObject.Cube(0, 0, 0, 1, 1, 1, Color.FromArgb(255, 90, 170, 240), 2);
    var satellite = SceneObject.Cube(1.15, 0.25, 0.2, 0.4, 0.4, 0.4, Color.FromArgb(255, 240, 150, 50), 2);
    body.AddChild(satellite);

    var poster = Bitmap2D.Checker(
        32,
        32,
        Color.FromArgb(255, 230, 40, 140),
        Color.FromArgb(255, 240, 220, 60),
        8);
    var sprite = new SceneObject { Position = new Point3(0.5, 1.25, 0.5) };
    sprite.Billboards.Add(new Billboard(new Point3(0, 0, 0), 0.7, 0.7, poster));
    body.AddChild(sprite);

    scene.Add(body);
    return scene;
}

static void VerifyGeometry()
{
    var cube = new Cube(0, 0, 0, 2, 3, 4);
    if (cube.Corners.Distinct().Count() != 8)
        throw new InvalidOperationException("Cube corners are not distinct.");
    if (cube.Edges().Count() != 12)
        throw new InvalidOperationException("Cube does not have 12 edges.");
    if (cube.Faces().Count() != 6)
        throw new InvalidOperationException("Cube does not have 6 faces.");
    if (cube.TopRightFront != new Point3(2, 3, 4) || cube.BottomLeftBack != new Point3(0, 0, 0))
        throw new InvalidOperationException("Cube corners do not span the requested box.");
}

static void VerifyViewport(Viewport viewport)
{
    var pixel = viewport.ToPixels(new Point2(0.6, 0.5));
    if (Math.Abs(pixel.X - 460) > 1e-6 || Math.Abs(pixel.Y - 300) > 1e-6)
        throw new InvalidOperationException($"Viewport aspect correction is wrong: {pixel}.");
}

static void VerifyProjection(Projection camera)
{
    if (Math.Abs(camera.CameraPointOfView.Yaw) > 1e-9 || Math.Abs(camera.CameraPointOfView.Pitch) > 1e-9)
        throw new InvalidOperationException("LookAt along +Z should leave yaw and pitch at zero.");

    var center = camera.Project(new Point3(0.5, 0.5, 0));
    if (Math.Abs(center.X - 0.5) > 1e-9 || Math.Abs(center.Y - 0.5) > 1e-9)
        throw new InvalidOperationException($"Center point projected to {center}.");

    var behind = camera.Project(new Point3(0.5, 0.5, -3));
    if (!double.IsNaN(behind.X) || !double.IsNaN(behind.Y))
        throw new InvalidOperationException("A point behind the camera should project to NaN.");
}

static void VerifyClip()
{
    var clipped = NearPlaneClip.Polygon(
    [
        new Point3(0, 0, -1),
        new Point3(1, 0, -1),
        new Point3(1, 0, 1),
        new Point3(0, 0, 1)
    ], 0.05);

    if (clipped.Count < 3 || clipped.Any(point => point.Z < 0.05 - 1e-9))
        throw new InvalidOperationException("Near-plane polygon clip kept points behind the plane.");

    if (!NearPlaneClip.Segment(new Point3(0, 0, -1), new Point3(0, 0, 1), 0.1, out var start, out var end))
        throw new InvalidOperationException("A segment crossing the near plane should survive.");
    if (Math.Abs(start.Z - 0.1) > 1e-9 || Math.Abs(end.Z - 1) > 1e-9)
        throw new InvalidOperationException($"Clipped segment was {start} -> {end}.");

    if (NearPlaneClip.Segment(new Point3(0, 0, -2), new Point3(0, 0, -1), 0.1, out _, out _))
        throw new InvalidOperationException("A segment fully behind the camera should be dropped.");
}

static void VerifyHierarchy()
{
    var parent = SceneObject.Cube(0, 0, 0, 1, 1, 1, Color.White);
    parent.Yaw = Math.PI / 2;
    var child = new SceneObject { Position = new Point3(1, 0, 0) };
    parent.AddChild(child);

    var world = child.TransformPoint(new Point3(0, 0, 0));
    if (Math.Abs(world.X) > 1e-3 || Math.Abs(world.Y) > 1e-3 || Math.Abs(world.Z + 1) > 1e-3)
        throw new InvalidOperationException($"Child world position was {world}, expected (0, 0, -1).");
}

static void VerifyOverlap()
{
    var box = new Cube(0, 0, 0, 1, 1, 1);
    var inside = new Cube(0.5, 0.5, 0.5, 1, 1, 1);
    var apart = new Cube(2, 0, 0, 1, 1, 1);
    var touching = new Cube(1, 0, 0, 1, 1, 1);

    if (!box.Overlaps(inside) || box.Overlaps(apart) || !box.Overlaps(touching))
        throw new InvalidOperationException("Cube overlap did not match the expected boxes.");
}

static void VerifyPick(Projection camera, Viewport viewport)
{
    var scene = new Scene();
    var cube = SceneObject.Cube(0, 0, 0, 1, 1, 1, Color.White);
    scene.Add(cube);

    var renderer = new SceneRenderer(camera, viewport);
    if (!renderer.TryPick(scene, new Point2(width / 2.0, height / 2.0), out var hit))
        throw new InvalidOperationException("The cube center should be pickable.");
    if (!ReferenceEquals(hit.Object, cube) || hit.Distance < 1.5 || hit.Distance > 2.5)
        throw new InvalidOperationException($"Unexpected pick {hit.FaceIndex} at distance {hit.Distance}.");
    if (renderer.TryPick(scene, new Point2(0, 0), out _))
        throw new InvalidOperationException("The corner of the frame should miss the cube.");
}

static int CountDiffering(Image<Rgba32> image, Color background)
{
    int count = 0;
    image.ProcessPixelRows(accessor =>
    {
        for (int y = 0; y < accessor.Height; y++)
        {
            var row = accessor.GetRowSpan(y);
            for (int x = 0; x < row.Length; x++)
            {
                var pixel = row[x];
                if (pixel.R != background.R || pixel.G != background.G || pixel.B != background.B || pixel.A != background.A)
                    count++;
            }
        }
    });
    return count;
}

static int CountColor(Image<Rgba32> image, Color color)
{
    int count = 0;
    image.ProcessPixelRows(accessor =>
    {
        for (int y = 0; y < accessor.Height; y++)
        {
            var row = accessor.GetRowSpan(y);
            for (int x = 0; x < row.Length; x++)
            {
                var pixel = row[x];
                if (pixel.R == color.R && pixel.G == color.G && pixel.B == color.B && pixel.A == color.A)
                    count++;
            }
        }
    });
    return count;
}
