using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

public class VisualCellResponsesDataSource
{
    public Vector<double> coefs { get; set; }
    public double sigma { get; set; }

    public Vector<double> GenerateWhiteNoise(int size)
    {
        // Create a vector of the specified size
        var vector = Vector<double>.Build.Dense(size);

        // Define a normal distribution with mean 0 and standard deviation 1
        var normalDist = new Normal(0, 1);

        // Fill the vector with random numbers from the normal distribution (white noise)
        for (int i = 0; i < size; i++)
        {
            vector[i] = normalDist.Sample();
        }

        return vector;
    }

    public IObservable<RegressionObservation> Process(IObservable<long> timerO)
    {
        Console.WriteLine("VisualCellResponsesDataSource::Process called");
        System.Random rng = SystemRandomSource.Default;

        return timerO.Select(time =>
            {
                Vector<double> phi = this.GenerateWhiteNoise(this.coefs.Count);
                double epsilon = Normal.Sample(0.0, this.sigma);
                double y = coefs.DotProduct(phi);
                double t = y + epsilon;

		        RegressionObservation observation = new RegressionObservation();
                observation.phi = phi;
                observation.t = t;

                return observation;
            });
    }
}
