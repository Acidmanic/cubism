namespace Cubism.Models;

public struct PointOfView
{
    public static readonly PointOfView DefaultCameraPointOfView = new PointOfView(0, 0, 1); 
    
    public readonly double Yaw;
    public readonly double Pitch;
    public readonly double FocalLength;

    public PointOfView(double yaw, double pitch, double focalLength)
    {
        Yaw = yaw;
        Pitch = pitch;
        FocalLength = focalLength;
    }

    public override string ToString()
    {
        return $"∡{Yaw},∡{Pitch},⤢{FocalLength}";
    }
}