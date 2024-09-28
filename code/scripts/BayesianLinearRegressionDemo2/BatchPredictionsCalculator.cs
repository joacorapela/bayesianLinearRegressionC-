using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Drawing;
using System.Drawing;
using System.Linq;


public class BatchPredictionsCalculator
{
    public double beta;

    public IObservable<(double[], double[])> Process(IObservable<(List<Vector<double>>, PosteriorDataItem)> source)
    {
        Console.WriteLine("BatchPredictionsCalculator::Process called");
        return source.Select(batchPhisAndPDI =>
            {
                List<Vector<double>> batchPhis = batchPhisAndPDI.Item1;
                PosteriorDataItem pdi = batchPhisAndPDI.Item2;
                double[] mean = new double[batchPhis.Count];
                double[] variance  = new double[batchPhis.Count];
                for (int i=0; i<batchPhis.Count; i++)
                {
                    (mean[i], variance[i]) = BayesianLinearRegression.Predict(phi: batchPhis[i], mn: pdi.mn, Sn: pdi.Sn, beta: this.beta);
                }
                return (mean, variance);
            });
    }
}

