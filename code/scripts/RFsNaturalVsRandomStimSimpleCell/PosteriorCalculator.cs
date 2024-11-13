using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using MathNet.Numerics.LinearAlgebra;
using System.Xml.Serialization;
using System.Globalization;

public class PosteriorCalculator
{
    public double priorPrecision { get; set; }

    public double likePrecision { get; set; }

    public double[] m0 { get; set; }

    public double[,] S0 { get; set; }

    public IObservable<PosteriorDataItem> Process(IObservable<RegressionObservation> source)
    {
        Console.WriteLine("PosteriorCalculator Process called");
        return source.Scan(
            new PosteriorDataItem
            {
                mn = Vector<double>.Build.DenseOfArray(m0),
                Sn = Matrix<double>.Build.DenseOfArray(S0)
            },
            (prior, observation) =>
            {
                ValueTuple<Vector<double>, Matrix<double>> tuple = BayesianLinearRegression.OnlineUpdate(prior.mn, prior.Sn, observation.phi, observation.t, priorPrecision, likePrecision);
		        Vector<double> mn = tuple.Item1;
		        Matrix<double> Sn = tuple.Item2;
		        PosteriorDataItem pdi = new PosteriorDataItem
                {
		            mn=mn,
		            Sn=Sn
                };
		        pdi.mn = mn;
		        pdi.Sn = Sn;
                return pdi;
            }
        );
    }
}
