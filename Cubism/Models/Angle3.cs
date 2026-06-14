namespace Cubism.Models;

public struct Angle3
{
    public readonly double Alpha;
    public readonly double Beta;
    public readonly double Gamma;

    public Angle3(double alpha, double beta, double gamma)
    {
        Alpha = alpha;
        Beta = beta;
        Gamma = gamma;
    }

    public override string ToString()
    {
        return $"∡{Alpha},∡{Beta},∡{Gamma}";
    }
}