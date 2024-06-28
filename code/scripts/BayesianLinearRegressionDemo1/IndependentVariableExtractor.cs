
using System;
using System.Reactive.Linq;

public class IndependentVariableExtractor
{
    IObservable<double> Process(IObservable<RegressionObservation> observationO)
    {
        return observationO.Select(observation => observation.x);
    }
}
