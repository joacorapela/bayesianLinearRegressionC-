using System;
using System.Reactive.Linq;

public class PosteriorVisualizer : IObserver<PosteriorDataItem>
{
    public void OnNext(PosteriorDataItem dataItem)
    {
        Console.WriteLine("PosteriorVisualizer::OnNext called");
        Console.WriteLine("mean = "+ dataItem.mn);
        Console.WriteLine("cov = "+ dataItem.Sn);
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

}

