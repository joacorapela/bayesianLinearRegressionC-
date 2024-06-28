using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

public class RegressionObservationsDataSource
{
	public double a0 { get; set; }
	public double a1 { get; set; }
	public double sigma { get; set; }
	public double srate { get; set; }

    public IObservable<RegressionObservation> Process(IObservable<long> source)
    {
        Console.WriteLine("RegressionObservationsDataSource::Process called");
        System.Random rng = SystemRandomSource.Default;

        return source.Select( input =>
        {
            Console.WriteLine("RegressionObservationsDataSource::Process generating a new observation");
            double x = rng.NextDouble();
            double epsilon = Normal.Sample(0.0, this.sigma);
            double y = this.a0 + this.a1 * x;
            double t = y + epsilon;

		    RegressionObservation observation = new RegressionObservation();
            observation.x = x;
            observation.t = t;

            return observation;
        });
    }
}
