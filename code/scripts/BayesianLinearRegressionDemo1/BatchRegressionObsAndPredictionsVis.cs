using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Drawing;
using System.Drawing;
using System.Linq;

public class BatchRegressionObsAndPredictionsVis : IObserver<(IList<RegressionObservation> batchRObs, PosteriorDataItem pdi)>
{
    public AvaPlot avaPlot1;
    public List<Func<double, double>> basisFunctions;
    public double beta;

    public void OnNext((IList<RegressionObservation> batchRObs, PosteriorDataItem pdi) batchROandPos)
    {
        Console.WriteLine("BatchRegressionObsAndPredictionsVis::OnNext called");
        List<RegressionObservation> batchRObs = (System.Collections.Generic.List<RegressionObservation>) batchROandPos.batchRObs;
        PosteriorDataItem pdi = batchROandPos.pdi;

        // computer predictions
        double[] x = new double[batchRObs.Count];
        double[] t = new double[batchRObs.Count];
        for (int i=0; i<batchRObs.Count; i++)
        {
            x[i] = batchRObs[i].x;
            t[i] = batchRObs[i].t;
        }

            // find xMin and xMax
        double xMin = x[0];
        double xMax = x[0];
        for(int i=1; i<x.Length; i++)
        {
            if (x[i] > xMax)
            {
                xMax = x[i];
            }
            if (x[i] < xMin)
            {
                xMin = x[i];
            }
        }

        int nDense = 100;
        double step = (xMax - xMin) / nDense;
        var xDense = Enumerable.Range(0, (int)Math.Ceiling((xMax - xMin) / step))
            .Select(i => xMin + i * step).ToArray();
        double[] mean = new double[xDense.Length];
        double[] variance  = new double[xDense.Length];
        for (int i=0; i<xDense.Length; i++)
        {
            var subsetBasisFunctions = new List<Func<double, double>>(this.basisFunctions.Take(pdi.mn.Count));
            var phiRow = RegressionUtils.BuildDesignMatrixRow(x: xDense[i], basisFunctions: subsetBasisFunctions);
            (mean[i], variance[i]) = BayesianLinearRegression.Predict(phi: phiRow, mn: pdi.mn, Sn: pdi.Sn, beta: this.beta);
        }

        // plot means and 95% ci for xDense
        var ci95Width = variance.Select(x=>1.96*Math.Sqrt(x)).ToArray();

        avaPlot1.Plot.Clear();

        avaPlot1.Plot.AddScatter(xDense, mean, Color.Blue, label: "Predictions");
        avaPlot1.Plot.AddFillError(xDense, mean, ci95Width, Color.FromArgb(50, Color.Blue));

        // plot data
        avaPlot1.Plot.AddScatter(x, t, Color.Red, lineWidth: 0, label: "Observations");
        avaPlot1.Plot.YLabel("f(x)");
        avaPlot1.Plot.XLabel("x");
        var legend = avaPlot1.Plot.Legend();
        legend.Location = Alignment.UpperLeft;


        avaPlot1.Refresh();
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

}

