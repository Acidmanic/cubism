using Avalonia.Controls;
using Avalonia.Media;
using Cubism.Avalonia;
using Cubism.Models;
using Cubism.Visual;
using Color = System.Drawing.Color;

namespace Cubism.Demo;

public class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Cubism — drag to orbit";
        Width = 960;
        Height = 640;
        Background = new SolidColorBrush(global::Avalonia.Media.Color.FromRgb(24, 26, 32));

        var scene = BuildScene(out var body);
        var camera = new Projection { CameraPosition = new Point3(2.2, 1.5, -2.6) };
        camera.LookAt(new Point3(0.7, 0.55, 0.4));

        var view = new CubismView
        {
            Scene = scene,
            Camera = camera
        };
        Content = view;
        new OrbitCameraController().Attach(view);

        view.Loaded += (_, _) =>
        {
            void Frame(TimeSpan time)
            {
                if (TopLevel.GetTopLevel(view) is not { } top)
                    return;

                body.Yaw = time.TotalSeconds * 0.6;
                view.InvalidateVisual();
                top.RequestAnimationFrame(Frame);
            }

            TopLevel.GetTopLevel(view)?.RequestAnimationFrame(Frame);
        };
    }

    private static Scene BuildScene(out SceneObject body)
    {
        var scene = new Scene();
        body = SceneObject.Cube(0, 0, 0, 1, 1, 1, Color.FromArgb(255, 90, 170, 240), 2);
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
}
