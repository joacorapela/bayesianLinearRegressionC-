
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class PosteriorCalculator
{
    private Vector<double> _m0;
    private Matrix<double> _S0;
    private double _priorPrecision;
    private double _likePrecision;

    public Vector<double> m0
    {
        get { return _m0; }
        set { _m0 = value; }
    }

    public Matrix<double> S0
    {
        get { return _S0; }
        set { _S0 = value; }
    }

    public double priorPrecision
    {
        get { return _priorPrecision; }
        set { _priorPrecision = value; }
    }

    public double likePrecision
    {
        get { return _likePrecision; }
        set { _likePrecision = value; }
    }

    public IObservable<PosteriorDataItem> Process(IObservable<RegressionObservation> source)
    {
        Console.WriteLine("PosteriorCalculator Process called");
        return source.Scan(
            new PosteriorDataItem
            {
                mn = m0,
                Sn = S0
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
