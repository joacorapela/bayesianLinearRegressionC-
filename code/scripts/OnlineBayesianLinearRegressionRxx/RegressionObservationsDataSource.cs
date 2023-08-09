
using System;
using System.Reactive;

public class RegressionObservationsDataSource: IObservable<RegressionObservation>
{
    private List<IObserver<RegressionObservation>> _observers;
	double[] _x;
	double[] _t;
	int _index;

    public bool done
    {
        get
        {
            // Console.WriteLine(String.Format("_x.Length{0:D2}, _t.Length={1:D2}, _index={2:D2}", _x.Length, _t.Length, _index));
            return (_index == _x.Length | _index == _t.Length);
        }
    }

    public RegressionObservationsDataSource(double[] x, double[] t)
    {
        _observers = new List<IObserver<RegressionObservation>>();

		_x = x;
		_t = t;
		_index = 0;
    }

    public IDisposable Subscribe(IObserver<RegressionObservation> observer)
    {
        _observers.Add(observer);
		IDisposable aDisposable = new MyDisposable(observer, _observers);
		return aDisposable;
    }

	public bool PublishNextObservation()
	{
		if (this._index < this._x.Length & this._index < this._t.Length)
		{
			RegressionObservation observation = new RegressionObservation();
            observation.x = this._x[this._index];
            observation.t = this._t[this._index];

			foreach (IObserver<RegressionObservation> observer in this._observers)
			{
				observer.OnNext(observation);
			}
			this._index++;
			return true;
		}
		else
		{
			foreach (IObserver<RegressionObservation> observer in this._observers)
			{
				observer.OnCompleted();
			}
			return false;
		}
	}
}

public class MyDisposable : IDisposable
{
    private readonly List<IObserver<RegressionObservation>> _observers;
    private readonly IObserver<RegressionObservation> _observer;
    private bool _disposed;

    public MyDisposable(IObserver<RegressionObservation> observer, List<IObserver<RegressionObservation>> observers)
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
