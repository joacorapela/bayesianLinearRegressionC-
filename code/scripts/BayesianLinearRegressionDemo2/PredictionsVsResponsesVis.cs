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


public class PredictionsVsResponsesVis : IObserver<(IList<RegressionObservation> batchRObs, PosteriorDataItem pdi)>
{
    public AvaPlot avaPlot1;
    public double beta;

    public void OnNext((IList<RegressionObservation> batchRObs, PosteriorDataItem pdi) batchROandPos)
    {
        Console.WriteLine("PredictionsVsResponsesVis::OnNext called");
        List<RegressionObservation> batchRObs = (System.Collections.Generic.List<RegressionObservation>) batchROandPos.batchRObs;
        PosteriorDataItem pdi = batchROandPos.pdi;

        // computer predictions
        double[] t = new double[batchRObs.Count];
        double[] mean = new double[batchRObs.Count];
        double[] variance  = new double[batchRObs.Count];
        for (int i=0; i<batchRObs.Count; i++)
        {
            (mean[i], variance[i]) = BayesianLinearRegression.Predict(phi: batchRObs[i].phi, mn: pdi.mn, Sn: pdi.Sn, beta: this.beta);
            t[i] = batchRObs[i].t;
        }
        // var ci95Width = variance.Select(x=>1.96*Math.Sqrt(x)).ToArray();

        // plot means and 95% ci for xDense
        avaPlot1.Plot.Clear();
        avaPlot1.Plot.AddScatter(t, mean, Color.Blue, lineWidth: 0);
        // avaPlot1.Plot.AddFillError(t, mean, ci95Width, Color.FromArgb(50, Color.Blue));

        // plot data
        avaPlot1.Plot.AddScatter(t, t, Color.Red, markerSize: 0);
        avaPlot1.Plot.YLabel("Predictions");
        avaPlot1.Plot.XLabel("Observations");

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

