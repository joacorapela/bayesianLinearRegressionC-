using System;
using System.Reactive.Linq;
using MathNet.Numerics.LinearAlgebra;

public class FileCoefsVisualizer : IObserver<Vector<double>>
{
    public void OnNext(Vector<double> coefs)
    {
        Console.WriteLine("FileCoefsVisualizer::OnNext called");
        Console.WriteLine("coefs = "+ coefs);
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

}

