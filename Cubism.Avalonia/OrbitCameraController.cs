using Cubism.Models;

namespace Cubism.Avalonia;

public sealed class OrbitCameraController
{
    private bool _dragging;
    private global::Avalonia.Point _last;

    public double RadiansPerPixel { get; set; } = 0.008;

    public void Attach(CubismView view)
    {
        view.PointerPressed += (_, e) =>
        {
            var point = e.GetCurrentPoint(view);
            if (!point.Properties.IsLeftButtonPressed)
                return;

            _dragging = true;
            _last = point.Position;
            e.Pointer.Capture(view);
        };

        view.PointerMoved += (_, e) =>
        {
            if (!_dragging || view.Camera is null)
                return;

            var position = e.GetCurrentPoint(view).Position;
            double dx = position.X - _last.X;
            double dy = position.Y - _last.Y;
            _last = position;

            var pov = view.Camera.CameraPointOfView;
            double pitch = Math.Clamp(pov.Pitch - dy * RadiansPerPixel, -Math.PI / 2 + 0.02, Math.PI / 2 - 0.02);
            view.Camera.CameraPointOfView = new PointOfView(
                pov.Yaw + dx * RadiansPerPixel,
                pitch,
                pov.FocalLength);
            view.InvalidateVisual();
        };

        view.PointerReleased += (_, e) =>
        {
            _dragging = false;
            e.Pointer.Capture(null);
        };
    }
}
