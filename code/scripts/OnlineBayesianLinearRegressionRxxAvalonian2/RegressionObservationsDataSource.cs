using System;
using System.Reactive;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using System.Threading;
using static System.Threading.Thread;

public class RegressionObservationsDataSource: IObservable<RegressionObservation>
{
    private List<IObserver<RegressionObservation>> _observers;
	public double a0 { get; set; }
	public double a1 { get; set; }
	public double sigma { get; set; }
	public double srate { get; set; }

    public RegressionObservationsDataSource()
    {
        Console.WriteLine("Constructor of RegressionObservationsDataSource called");
        _observers = new List<IObserver<RegressionObservation>>();
    }

    public IDisposable Subscribe(IObserver<RegressionObservation> observer)
    {
        Console.WriteLine("RegressionObservationsDataSource::Subscribe called");
        _observers.Add(observer);
		RegressionObservationODisposable disposable = new RegressionObservationODisposable(observer, _observers);
        var thread = new Thread(() => PublishObservations(observer, disposable));
        thread.Start();
		return disposable;
    }

    public void PublishObservations(IObserver<RegressionObservation> observer, RegressionObservationODisposable disposable)
    {
        System.Random rng = SystemRandomSource.Default;
        while (!disposable.isDisposed())
        {
            double x = rng.NextDouble();
            double epsilon = Normal.Sample(0.0, this.sigma);
            double y = this.a0 + this.a1 * x;
            double t = y + epsilon;

		    RegressionObservation observation = new RegressionObservation();
            observation.x = x;
            observation.t = t;
			observer.OnNext(observation);
            Console.WriteLine("RegressionObservationsDataSource observation published");

            Sleep(TimeSpan.FromSeconds(1.0/this.srate));
			observer.OnNext(observation);
        }
    }

	public void PublishNextObservation()
	{
        Console.WriteLine("RegressionObservationsDataSource::PublishNextObservation called");
        System.Random rng = SystemRandomSource.Default;
        double x = rng.NextDouble();
        double epsilon = Normal.Sample(0.0, this.sigma);
        double y = this.a0 + this.a1 * x;
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
        _disposed = false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _observers.Remove(_observer);
            _disposed = true;
        }
    }

    public bool isDisposed()
    {
            return _disposed;
    }
}
