
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class PosteriorCalculator
{
    private Vector<double> _mn;
    private Matrix<double> _Sn;
    private double _alpha;
    private double _beta;

    public Vector<double> mn
    {
        get { return _mn; }
        set { _mn = value; }
    }

    public Matrix<double> Sn
    {
        get { return _Sn; }
        set { _Sn = value; }
    }

    public PosteriorCalculator(double alpha, double beta, Vector<double> m0, Matrix<double> S0)
    {
        _mn = m0;
        _Sn = S0;
        _alpha = alpha;
        _beta = beta;
        Console.WriteLine("Constructor of PosteriorCalculator called");
    }

    public IObservable<PosteriorDataItem> Process(IObservable<RegressionObservation> source)
    {
        Console.WriteLine("PosteriorCalculator Process called");
        return source.Select(observation =>
        {
            Console.WriteLine("new posterior calculated");
            double x = observation.x;
            double t = observation.t;
            double[] aux = new[] {1, x};
            Vector<double> phi = Vector<double>.Build.DenseOfArray(aux);
            var res = BayesianLinearRegression.OnlineUpdate(_mn, _Sn, phi, t, _alpha, _beta);
            mn = res.mean;
            Sn = res.cov;
            var pdi = new PosteriorDataItem();
            pdi.mn = mn;
            pdi.Sn = Sn;
            return pdi;
        });
    }
}
