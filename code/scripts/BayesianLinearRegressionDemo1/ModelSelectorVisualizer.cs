using System;
using System.Reactive.Linq;

public class ModelSelectorVisualizer : IObserver<int>
{
    public void OnNext(int M)
    {
        Console.WriteLine("ModelSelectorVisualizer::OnNext called");
        Console.WriteLine("M = "+ M);
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

}

