
using System;
using System.Reactive;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class OnlineBayesianLinearRegression : IObserver<RegressionObservation>, IObservable<PosteriorDataItem>
{
    private List<IObserver<PosteriorDataItem>> _observers;
    // private Vector<double> _mn;
    // private Matrix<double> _Sn;
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

    public OnlineBayesianLinearRegression(double alpha, double beta, Vector<double> m0, Matrix<double> S0)
    {
        _observers = new List<IObserver<PosteriorDataItem>>();
        _mn = m0;
        _Sn = S0;
        _alpha = alpha;
        _beta = beta;
        Console.WriteLine("Constructor of OnlineBayesianLinearRegression called");
    }

    public IDisposable Subscribe(IObserver<PosteriorDataItem> observer)
    {
        _observers.Add(observer);
		IDisposable aDisposable = new PosteriorDataItemODisposable(observer, _observers);
        Console.WriteLine("OnlineBayesianLinearRegression::Subscribe called");
		return aDisposable;
    }

    public void OnNext(RegressionObservation observation)
    {
        Console.WriteLine("OnlineBayesianLinearRegression::OnNext called");
        double x = observation.x;
        double t = observation.t;
        double[] aux = new[] {1, x};
        Vector<double> phi = Vector<double>.Build.DenseOfArray(aux);
        var res = BayesianLinearRegression.OnlineUpdate(_mn, _Sn, phi, t, _alpha, _beta);
        _mn = res.mean;
        _Sn = res.cov;
        var pdi = new PosteriorDataItem();
        pdi.mn = mn;
        pdi.Sn = Sn;
		foreach (IObserver<PosteriorDataItem> observer in _observers)
		{
			observer.OnNext(pdi);
		}
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

}

public class PosteriorDataItemODisposable : IDisposable
{
    private readonly List<IObserver<PosteriorDataItem>> _observers;
    private readonly IObserver<PosteriorDataItem> _observer;
    private bool _disposed;

    public PosteriorDataItemODisposable(IObserver<PosteriorDataItem> observer, List<IObserver<PosteriorDataItem>> observers)
    {
        _observer = observer;
        _observers = observers;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _observers.Remove(_observer);
            _disposed = true;
        }
    }
}
