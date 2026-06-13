using Cubism.Mathematics.Exceptions;

namespace Cubism.Mathematics;

public struct FastMatrix
{
    private double[,] _values;
    private readonly int _rows;
    private readonly int _columns;

    public FastMatrix(double[,] values)
    {
        _values = values;
        _rows = values.GetLength(0);
        _columns = values.GetLength(1);
    }

    public FastMatrix(int rows, int columns)
    {
        _values = new double[rows, columns];
        _rows = rows;
        _columns = columns;
    }

    public double this[int row, int col]
    {
        get => _values[row, col];
        set => _values[row, col] = value;
    }

    public static FastMatrix Ones(int rows, int columns)
    {
        var m = new FastMatrix(rows, columns);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                m._values[r, c] = m[r, c];
            }
        }

        return m;
    }

    public static FastMatrix Zero(int rows, int columns)
    {
        var m = new FastMatrix(rows, columns);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                m._values[r, c] = m[r, c];
            }
        }

        return m;
    }

    public static FastMatrix Random(int rows, int columns)
    {
        var m = new FastMatrix(rows, columns);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                m._values[r, c] = System.Random.Shared.NextDouble();
            }
        }

        return m;
    }


    public static FastMatrix operator *(FastMatrix m1, FastMatrix m2)
    {
        if (m1._columns != m2._rows) throw new DimensionMismatchException();

        var length = m1._columns;

        var result = new FastMatrix(m1._rows, m2._columns);

        for (int r = 0; r < m1._rows; r++)
        {
            for (int c = 0; c < m2._columns; c++)
            {
                for (int i = 0; i < length; i++)
                {
                    result[r, c] += m1[r, i] * m2[i, c];
                }
            }
        }

        return result;
    }
}