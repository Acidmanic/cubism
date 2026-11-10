namespace Cubism.Models;

public static class Location
{
    public static bool Equals(double v1, double v2,double pixelsInUnit)
    {
        return Math.Abs(v1 - v2) < 1/pixelsInUnit;
    }
    

}