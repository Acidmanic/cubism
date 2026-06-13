using Cubism.Mathematics;

Console.WriteLine("Hello World!");



var m1 = new FastMatrix(new double[,]
{
    { 1, 2 },
    { 3, 4 }
});

var m2 = new FastMatrix(new double[,]
    {
        { 1, 2 },
        { 3, 4 }
    }
);

var m3 = m1*m2;

Console.WriteLine();