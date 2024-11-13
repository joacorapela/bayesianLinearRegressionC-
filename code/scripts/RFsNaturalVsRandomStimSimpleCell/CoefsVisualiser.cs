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
using MathNet.Numerics.LinearAlgebra;

public class CoefsVisualiser: IObserver<PosteriorDataItem>
{
    public double[] coefs { get; set; }

    public AvaPlot window;

    public void OnNext(PosteriorDataItem pdi)
    {
        Console.WriteLine("CoefsVisualiser::OnNext called");
        double[] posteriorMean = pdi.mn.ToArray();
        double[] posterior95CI = (pdi.Sn.Diagonal().PointwisePower(0.5) * 1.96).ToArray();

        window.Plot.Clear();
        double[] xs = DataGen.Consecutive(posteriorMean.Count());
		var bar = window.Plot.AddBar(posteriorMean);
        bar.ValueErrors = posterior95CI;

        window.Plot.XLabel("Index");
        window.Plot.YLabel("Coefficient");

		// customize the plot to make it look nicer
        window.Plot.XAxis.Grid(false); // Disable vertical grid lines
        window.Plot.YAxis.Grid(true);
		window.Plot.Legend(location: Alignment.UpperCenter);

        window.Refresh();
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }
}

