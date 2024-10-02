using System;
using System.Reactive.Linq;
using MathNet.Numerics.LinearAlgebra;

public class PredictionsCalculator
{
    public double beta;

    public IObservable<(double, double)> Process(IObservable<(Vector<double>, PosteriorDataItem)> source)
     {
        Console.WriteLine("PredictionsCalculator::Process called");
	return source.Select(phiAndPDI => BayesianLinearRegression.Predict(phi: phiAndPDI.Item1, mn: phiAndPDI.Item2.mn, Sn: phiAndPDI.Item2.Sn, beta: this.beta));
     }
}
