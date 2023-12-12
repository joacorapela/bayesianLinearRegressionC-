
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class PosteriorCalculator
{
    public double[] m0
    {
        get;
        set;
    }

    public double[,] S0
    {
        get;
        set;
    }

    public double priorPrecision
    {
        get;
        set;
    }

    public double likePrecision
    {
        get;
        set;
    }

    public IObservable<PosteriorDataItem> Process(IObservable<RegressionObservation> source)
    {
        Console.WriteLine("PosteriorCalculator Process called");
        return source.Scan(
            new PosteriorDataItem
            {
                mn = Vector<double>.Build.DenseOfArray(m0),
                Sn = Matrix<double>.Build.DenseOfArray(S0)
            },
            (prior, observation) => 
            {
                double[] aux = new[] { 1, observation.x };
                Vector<double> phi = Vector<double>.Build.DenseOfArray(aux);
                var post = BayesianLinearRegression.OnlineUpdate(prior.mn, prior.Sn, phi, observation.t, priorPrecision, likePrecision);
                PosteriorDataItem pdi = new PosteriorDataItem { mn = post.mean, Sn = post.cov };
                return pdi;
            });

    }
}
