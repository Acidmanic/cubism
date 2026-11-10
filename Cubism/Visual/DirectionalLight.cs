using Cubism.Models;

namespace Cubism.Visual;

public readonly struct DirectionalLight
{
    public Point3 Direction { get; }
    public double Intensity { get; }

    public DirectionalLight(Point3 directionTowardLight, double intensity)
    {
        Direction = directionTowardLight;
        Intensity = intensity;
    }

    public static DirectionalLight Default { get; } = new(new Point3(-0.45, 0.85, -0.25), 1);
}
