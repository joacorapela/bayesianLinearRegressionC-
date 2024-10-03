using System;
using System.Reactive.Linq;
using MathNet.Numerics.LinearAlgebra;

public class PredictionsCalculator
{
    public double beta;

    // public IObservable<(double, double)> Process(IObservable<(Vector<double>, PosteriorDataItem)> source)
    public IObservable<(double, double, double)> Process(IObservable<(RegressionObservation, PosteriorDataItem)> source)
     {
        Console.WriteLine("PredictionsCalculator::Process called");
        // IObservable<(double, double)> answer = source.Select(phiAndPDI => BayesianLinearRegression.Predict(phi: phiAndPDI.Item1, mn: phiAndPDI.Item2.mn, Sn: phiAndPDI.Item2.Sn, beta: this.beta));
        IObservable<(double, double, double)> answer = source.Select(phiAndPDI => 
        {
            var prediction = BayesianLinearRegression.Predict(phi: phiAndPDI.Item1.phi, mn: phiAndPDI.Item2.mn, Sn: phiAndPDI.Item2.Sn, beta: this.beta);
            Console.WriteLine($"prediction.mean={prediction.Item1}, observation={phiAndPDI.Item1.t}");
            var answer2 = (prediction.Item1, prediction.Item2, phiAndPDI.Item1.t);
            return answer2;
        });
        return answer;
     }
}
