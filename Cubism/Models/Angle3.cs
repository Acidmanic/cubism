namespace Cubism.Models;

public struct Angle3
{
    public double Alpha { get;  }
    public double Beta { get;  }
    public double Gamma { get;  }

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