using Cubism.Visual;
using Control = global::Avalonia.Controls.Control;
using DrawingContext = global::Avalonia.Media.DrawingContext;

namespace Cubism.Avalonia;

public class CubismView : Control
{
    private Scene? _scene;
    private Projection? _camera;

    public CubismView()
    {
        ClipToBounds = true;
    }

    public Scene? Scene
    {
        get => _scene;
        set
        {
            _scene = value;
            InvalidateVisual();
        }
    }

    public Projection? Camera
    {
        get => _camera;
        set
        {
            _camera = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_scene is null || _camera is null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var renderer = new SceneRenderer(_camera, new Viewport(Bounds.Width, Bounds.Height));
        renderer.Render(_scene, new AvaloniaRenderContext2D(context));
    }
}
