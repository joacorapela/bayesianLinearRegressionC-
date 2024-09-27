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

public class CoefsAndPosteriorVis: IObserver<PosteriorDataItem>
{
    public double[] coefs { get; set; }

    public AvaPlot window;

    public void OnNext(PosteriorDataItem pdi)
    {
        Console.WriteLine("CoefsAndPosteriorVis::OnNext called");
        double[] coefs95PCI = new double[this.coefs.Count()];
        double[] posteriorMean = pdi.mn.ToArray();
        double[] posterior95CI = (pdi.Sn.Diagonal().PointwisePower(0.5) * 1.96).ToArray();

        double[] coefsToPlot;
        double[] coefs95PCIToPlot;
        double[] posteriorMeanToPlot;
        double[] posterior95CIToPlot;

        coefsToPlot = this.coefs;
        coefs95PCIToPlot = coefs95PCI;
        posteriorMeanToPlot = posteriorMean;
        posterior95CIToPlot = posterior95CI;

        string[] groupNames = Enumerable.Range(0, this.coefs.Count()).Select(d => d.ToString()).ToArray();

		string[] seriesNames = { "true", "estimated" };

        window.Plot.Clear();

        double[][] ys_tmp = { coefsToPlot, posteriorMeanToPlot };
        double[][] yErr_tmp = { coefs95PCIToPlot, posterior95CIToPlot };
		window.Plot.PlotBarGroups(
    		groupLabels: groupNames,
    		seriesLabels: seriesNames,
    		ys: ys_tmp,
    		yErr: yErr_tmp);
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

