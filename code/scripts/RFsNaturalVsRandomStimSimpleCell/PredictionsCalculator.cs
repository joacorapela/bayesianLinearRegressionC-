using System;
using System.Reactive.Linq;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class PredictionsCalculator
{
    public double beta;

    public IObservable<IList<(double, double)>> Process(IObservable<(PosteriorDataItem, IList<Vector<double>>)> pdiAndBatchPhisO)
    {
        Console.WriteLine("PredictionsCalculator::Process called");
        IObservable<IList<(double, double)>> answer = pdiAndBatchPhisO.Select(
            pdiAndBatchPhis =>
            {
                PosteriorDataItem pdi = pdiAndBatchPhis.Item1;
                IList<Vector<double>> phis = pdiAndBatchPhis.Item2;
                List <(double, double)> predictions = new List<(double, double)>();
                foreach (Vector<double> phi in phis)
                {
                    var prediction = BayesianLinearRegression.Predict(phi: phi, mn: pdi.mn, Sn: pdi.Sn, beta: this.beta);
                    predictions.Add(prediction);
                }
                // var predictions = phis.Select(phi => BayesianLinearRegression.Predict(phi: phi, mn: pdi.mn, Sn: pdi.Sn, beta: this.beta));
                return predictions;
            });
        return answer;
     }
}
