using System;
using System.Reactive.Linq;

public class MLLvisualizer : IObserver<double>
{
    public void OnNext(double ll)
    {
        Console.WriteLine("MLLvisualizer::OnNext called");
        Console.WriteLine("ll = "+ ll);
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

}

