
using System;
using System.Reactive;
using MathNet.Numerics.LinearAlgebra;

public class OnlineBayesianLinearRegression : IObserver<RegressionObservation>
{
    // private Vector<double> _mn;
    // private Matrix<double> _Sn;
    public Vector<double> mn;
    public Matrix<double> Sn;
    private double _alpha;
    private double _beta;

    public OnlineBayesianLinearRegression(double alpha, double beta, Vector<double> m0, Matrix<double> S0)
    {
        mn = m0;
        Sn = S0;
        _alpha = alpha;
        _beta = beta;
    }

    /*
    public Vector<double> mn
    {
        get => _mn;
    }

    public Matrix<double> Sn
    {
        get => _Sn;
    }
    */

    public void OnNext(RegressionObservation observation)
    {
        double x = observation.x;
        double t = observation.t;
        double[] aux = new[] {1, x};
        Vector<double> phi = Vector<double>.Build.DenseOfArray(aux);
        var res = BayesianLinearRegression.OnlineUpdate(mn, Sn, phi, t, _alpha, _beta);
        mn = res.mean;
        Sn = res.cov;
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

}
