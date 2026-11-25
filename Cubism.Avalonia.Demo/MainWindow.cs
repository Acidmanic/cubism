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
        Title = "Cubism — right-drag look, WASD fly, wheel dolly";
        Width = 960;
        Height = 640;
        Background = new SolidColorBrush(global::Avalonia.Media.Color.FromRgb(24, 26, 32));

        var camera = new Projection { CameraPosition = new Point3(3.2, 1.7, -6) };
        camera.LookAt(new Point3(5.6, 1.2, 6.2));

        var view = new CubismView
        {
            Scene = BuildScene(),
            Camera = camera
        };
        Content = view;
        new ExploreCameraController().Attach(view);
    }

    private static Scene BuildScene()
    {
        var scene = new Scene();
        const double wall = 0.3;
        const double room = 8;
        const double height = 4;

        scene.Add(Ground());
        scene.Add(Wall(0, 0, 0, wall, height, room, Rgb(150, 158, 168)));
        scene.Add(Wall(room - wall, 0, 0, wall, height, room, Rgb(176, 154, 132)));
        scene.Add(Wall(0, 0, room - wall, room, height, wall, Rgb(132, 124, 116)));
        scene.Add(Wall(0, 0, 0, 2.7, height, wall, Rgb(196, 184, 164)));
        scene.Add(Wall(5.3, 0, 0, room - 5.3, height, wall, Rgb(196, 184, 164)));
        scene.Add(Wall(2.7, 2.5, 0, 2.6, height - 2.5, wall, Rgb(196, 184, 164)));
        scene.Add(Wall(0, height - wall, 0, room, wall, room, Rgb(108, 104, 100)));

        double box = 1.2;
        double x = room - wall - box;
        double z = room - wall - box;
        scene.Add(Crate(x, 0, z, box, 0.85, Rgb(176, 74, 58)));
        scene.Add(Crate(x, 0.85, z, box, 0.85, Rgb(232, 186, 74)));
        scene.Add(Crate(x, 1.7, z, box, 0.85, Rgb(64, 122, 168)));
        return scene;
    }

    private static SceneObject Ground()
    {
        var fill = new ImageFill(Bitmap2D.Checker(64, 64, Rgb(74, 92, 64), Rgb(118, 132, 86), 8));
        var ground = new SceneObject();
        ground.Faces.Add(new Face(
            fill,
            1,
            new Point3(-8, 0, -8),
            new Point3(-8, 0, 16),
            new Point3(16, 0, 16),
            new Point3(16, 0, -8))
        {
            Stroke = false
        });
        return ground;
    }

    private static SceneObject Wall(double x, double y, double z, double width, double height, double depth, Color color)
        => SceneObject.Cube(x, y, z, width, height, depth, color, 1.5);

    private static SceneObject Crate(double x, double y, double z, double size, double height, Color color)
        => SceneObject.Cube(x, y, z, size, height, size, color, 1.5);

    private static Color Rgb(int r, int g, int b) => Color.FromArgb(255, r, g, b);
}
