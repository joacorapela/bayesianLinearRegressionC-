using System;
using System.Reactive;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

public class RegressionObservationsDataSource: IObservable<RegressionObservation>
{
    private List<IObserver<RegressionObservation>> _observers;
	double _a0;
	double _a1;
    double _sigma;

    public bool done
    {
        get
        {
            // Console.WriteLine(String.Format("_x.Length{0:D2}, _t.Length={1:D2}, _index={2:D2}", _x.Length, _t.Length, _index));
            return false;
        }
    }

    public RegressionObservationsDataSource(double a0, double a1, double sigma)
    {
        Console.WriteLine("Constructor of RegressionObservationsDataSource called");
        _observers = new List<IObserver<RegressionObservation>>();

		this._a0 = a0;
		this._a1 = a1;
		this._sigma = sigma;
    }

    public IDisposable Subscribe(IObserver<RegressionObservation> observer)
    {
        Console.WriteLine("RegressionObservationsDataSource::Subscribe called");
        _observers.Add(observer);
		IDisposable aDisposable = new RegressionObservationODisposable(observer, _observers);
		return aDisposable;
    }

	public void PublishNextObservation()
	{
        Console.WriteLine("RegressionObservationsDataSource::PublishNextObservation called");
        System.Random rng = SystemRandomSource.Default;
        double x = rng.NextDouble();
        double epsilon = Normal.Sample(0.0, this._sigma);
        double y = this._a0 + this._a1 * x;
        double t = y + epsilon;

		RegressionObservation observation = new RegressionObservation();
        observation.x = x;
        observation.t = t;

		foreach (IObserver<RegressionObservation> observer in this._observers)
		{
			observer.OnNext(observation);
		}
	}
}

public class RegressionObservationODisposable : IDisposable
{
    private readonly List<IObserver<RegressionObservation>> _observers;
    private readonly IObserver<RegressionObservation> _observer;
    private bool _disposed;

    public RegressionObservationODisposable(IObserver<RegressionObservation> observer, List<IObserver<RegressionObservation>> observers)
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
