using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

public class VisualCellResponsesDataSource
{
    public Matrix<double> images { get; set; }
    public Vector<double> responses { get; set; }

    public IObservable<RegressionObservation> Process(IObservable<long> timerO)
    {
        Console.WriteLine("VisualCellResponsesDataSource::Process called");
        System.Random rng = SystemRandomSource.Default;
        int n = 0;
        int maxIndex = Math.Min(images.RowCount, responses.Count); // Set max bounds
        return timerO.Select(time =>
        {
			Console.WriteLine($"Providing visual cell response number: {n}");
            if (n >= maxIndex)
            {
                Console.WriteLine("Reached end of data, stopping sequence.");
                return null; // You can change this if you prefer to handle it differently
            }

            RegressionObservation observation = new RegressionObservation();
            observation.phi = images.Row(n);
            observation.t = responses.At(n);
            n = n + 1;
            return observation;
        })
        .TakeWhile(observation => observation != null);
    }
}
