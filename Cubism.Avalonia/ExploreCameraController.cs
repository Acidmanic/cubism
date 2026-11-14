using Cubism.Models;
using Cubism.Visual;
using AvaloniaKey = global::Avalonia.Input.Key;
using Cursor = global::Avalonia.Input.Cursor;
using StandardCursorType = global::Avalonia.Input.StandardCursorType;
using KeyModifiers = global::Avalonia.Input.KeyModifiers;
using TopLevel = global::Avalonia.Controls.TopLevel;

namespace Cubism.Avalonia;

/// <summary>
/// Fly camera. Right-drag looks, middle-drag pans, the wheel dollies.
/// WASD or the arrows fly along the view, Q/E or Space/Ctrl move vertically, Shift speeds up.
/// Shift plus the wheel changes the fly speed. Movement eases in and out.
/// </summary>
public sealed class ExploreCameraController
{
    private readonly HashSet<AvaloniaKey> _keys = new();
    private bool _looking;
    private bool _panning;
    private bool _looping;
    private global::Avalonia.Point _last;
    private TimeSpan? _lastTick;
    private Point3 _velocity;
    private Cursor? _savedCursor;

    public double MoveSpeed { get; set; } = 2.5;
    public double FastMultiplier { get; set; } = 3;
    public double LookRadiansPerPixel { get; set; } = 0.005;
    public double PanUnitsPerPixel { get; set; } = 0.01;
    public double WheelDolly { get; set; } = 0.35;
    public double Responsiveness { get; set; } = 12;

    public void Attach(CubismView view)
    {
        view.Focusable = true;

        view.PointerPressed += (_, e) =>
        {
            var point = e.GetCurrentPoint(view);
            var props = point.Properties;
            if (!props.IsRightButtonPressed && !props.IsMiddleButtonPressed)
            {
                view.Focus();
                return;
            }

            if (props.IsRightButtonPressed && !_looking)
            {
                _looking = true;
                _savedCursor = view.Cursor;
                view.Cursor = new Cursor(StandardCursorType.None);
            }

            _panning = props.IsMiddleButtonPressed;
            _last = point.Position;
            e.Pointer.Capture(view);
            view.Focus();
        };

        view.PointerMoved += (_, e) =>
        {
            if (view.Camera is null || (!_looking && !_panning))
                return;

            var position = e.GetCurrentPoint(view).Position;
            double dx = position.X - _last.X;
            double dy = position.Y - _last.Y;
            _last = position;

            if (_looking)
                Look(view.Camera, dx, dy);
            else
                Pan(view.Camera, dx, dy);

            view.InvalidateVisual();
        };

        view.PointerReleased += (_, e) =>
        {
            var props = e.GetCurrentPoint(view).Properties;
            if (!props.IsRightButtonPressed && _looking)
            {
                _looking = false;
                view.Cursor = _savedCursor;
            }

            _panning = props.IsMiddleButtonPressed;
            if (!_looking && !_panning)
                e.Pointer.Capture(null);
        };

        view.PointerWheelChanged += (_, e) =>
        {
            if (view.Camera is null)
                return;

            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                MoveSpeed = Math.Clamp(MoveSpeed * (e.Delta.Y > 0 ? 1.15 : 1 / 1.15), 0.2, 40);
            else
                view.Camera.CameraPosition += Forward(view.Camera) * (e.Delta.Y * WheelDolly);

            view.InvalidateVisual();
            e.Handled = true;
        };

        view.KeyDown += (_, e) =>
        {
            if (e.Key == AvaloniaKey.Escape)
            {
                _looking = false;
                _panning = false;
                view.Cursor = _savedCursor;
                _keys.Clear();
                return;
            }

            _keys.Add(e.Key);
        };

        view.KeyUp += (_, e) => _keys.Remove(e.Key);
        view.LostFocus += (_, _) => _keys.Clear();
        view.Loaded += (_, _) => EnsureLoop(view);
        view.AttachedToVisualTree += (_, _) => EnsureLoop(view);
    }

    private void EnsureLoop(CubismView view)
    {
        if (_looping || TopLevel.GetTopLevel(view) is not { } top)
            return;

        _looping = true;
        top.RequestAnimationFrame(time => Tick(view, time));
    }

    private void Tick(CubismView view, TimeSpan time)
    {
        if (TopLevel.GetTopLevel(view) is not { } top)
        {
            _looping = false;
            return;
        }

        if (_lastTick is TimeSpan previous)
        {
            double dt = Math.Clamp((time - previous).TotalSeconds, 0, 0.05);
            if (dt > 0)
                Fly(view, dt);
        }

        _lastTick = time;
        top.RequestAnimationFrame(next => Tick(view, next));
    }

    private void Fly(CubismView view, double dt)
    {
        if (view.Camera is null)
            return;

        double forwardAxis = Axis(AvaloniaKey.W, AvaloniaKey.Up) - Axis(AvaloniaKey.S, AvaloniaKey.Down);
        double rightAxis = Axis(AvaloniaKey.D, AvaloniaKey.Right) - Axis(AvaloniaKey.A, AvaloniaKey.Left);
        double upAxis = Axis(AvaloniaKey.E, AvaloniaKey.Space) - Axis(AvaloniaKey.Q, AvaloniaKey.LeftCtrl, AvaloniaKey.RightCtrl, AvaloniaKey.C);

        var wish = new Point3(0, 0, 0);
        if (forwardAxis != 0)
            wish += Forward(view.Camera) * forwardAxis;
        if (rightAxis != 0)
            wish += FlatRight(view.Camera.CameraPointOfView.Yaw) * rightAxis;
        if (upAxis != 0)
            wish += new Point3(0, upAxis, 0);

        if (wish.Length > 1e-8)
            wish = wish.Normalized();

        bool fast = _keys.Contains(AvaloniaKey.LeftShift) || _keys.Contains(AvaloniaKey.RightShift);
        var target = wish * (MoveSpeed * (fast ? FastMultiplier : 1));
        double blend = 1 - Math.Exp(-Responsiveness * dt);
        _velocity += (target - _velocity) * blend;

        if (_velocity.Length < 1e-4)
        {
            _velocity = default;
            return;
        }

        view.Camera.CameraPosition += _velocity * dt;
        view.InvalidateVisual();
    }

    private void Look(Projection camera, double dx, double dy)
    {
        var pov = camera.CameraPointOfView;
        double pitch = Math.Clamp(pov.Pitch - dy * LookRadiansPerPixel, -Math.PI / 2 + 0.02, Math.PI / 2 - 0.02);
        camera.CameraPointOfView = new PointOfView(pov.Yaw + dx * LookRadiansPerPixel, pitch, pov.FocalLength);
    }

    private void Pan(Projection camera, double dx, double dy)
    {
        var right = FlatRight(camera.CameraPointOfView.Yaw);
        var up = camera.CameraDirectionToWorld(new Point3(0, 1, 0)).Normalized();
        camera.CameraPosition += right * (-dx * PanUnitsPerPixel) + up * (dy * PanUnitsPerPixel);
    }

    private double Axis(params AvaloniaKey[] keys)
    {
        foreach (var key in keys)
        {
            if (_keys.Contains(key))
                return 1;
        }

        return 0;
    }

    private static Point3 Forward(Projection camera)
    {
        var direction = camera.CameraDirectionToWorld(new Point3(0, 0, 1));
        return direction.Length <= 1e-8 ? new Point3(0, 0, 1) : direction.Normalized();
    }

    private static Point3 FlatRight(double yaw) => new(Math.Cos(yaw), 0, -Math.Sin(yaw));
}
