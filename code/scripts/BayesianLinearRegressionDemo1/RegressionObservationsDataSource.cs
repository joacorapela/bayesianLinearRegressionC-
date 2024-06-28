using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

public class RegressionObservationsDataSource
{
    public List<Func<double, double>> basisFunctions { get; set; }
    public double sigma { get; set; }

    public IObservable<RegressionObservation> Process(IObservable<long> timerO, IObservable<Vector<double>> coefsO)
    {
        Console.WriteLine("RegressionObservationsDataSource::Process called");
        System.Random rng = SystemRandomSource.Default;

        return timerO.CombineLatest(coefsO,
            (time, coefs) =>
            {
                // Console.WriteLine($"RegressionObservationsDataSource::Process generating a new observation with {coefs.Count} coefs");
                double x = rng.NextDouble() * 4 - 2;
                Vector<double> xBasisFuncExpansion = RegressionUtils.BuildDesignMatrixRow(x, this.basisFunctions.GetRange(0, coefs.Count));
                double epsilon = Normal.Sample(0.0, this.sigma);
                double y = coefs.DotProduct(xBasisFuncExpansion);
                double t = y + epsilon;

		        RegressionObservation observation = new RegressionObservation();
                observation.x = x;
                observation.t = t;

                return observation;
            });
    }
}
