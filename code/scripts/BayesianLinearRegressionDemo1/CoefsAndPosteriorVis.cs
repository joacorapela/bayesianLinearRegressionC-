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

public class CoefsAndPosteriorVis: IObserver<CoefsAndPosteriorDataItem>
{
    public AvaPlot avaPlot2;

    public void OnNext(CoefsAndPosteriorDataItem coefsAndPostDI)
    {
        Console.WriteLine("CoefsAndPosteriorVis::OnNext called");
        Vector<double> coefs = coefsAndPostDI.coefs;
        PosteriorDataItem pdi = coefsAndPostDI.pdi;

        double[] coefs95PCI = new double[coefs.Count()];
        double[] posteriorMean = pdi.mn.ToArray();
        double[] posterior95CI = (pdi.Sn.Diagonal().PointwisePower(0.5) * 1.96).ToArray();

        double[] coefsToPlot;
        double[] coefs95PCIToPlot;
        double[] posteriorMeanToPlot;
        double[] posterior95CIToPlot;

        int maxNCoefs = Math.Max(coefs.Count(), posteriorMean.Count());
        if (maxNCoefs > coefs.Count())
        {
            coefsToPlot = new double[maxNCoefs];
            coefs95PCIToPlot = new double[maxNCoefs];
            for (int i=0; i<coefs.Count(); i++)
            {
                coefsToPlot[i] = coefs[i];
            }
        }
        else
        {
            coefsToPlot = coefs.ToArray();
            coefs95PCIToPlot = coefs95PCI;
        }
        if (maxNCoefs > posteriorMean.Count())
        {
            posteriorMeanToPlot = new double[maxNCoefs];
            posterior95CIToPlot = new double[maxNCoefs];
            for (int i=0; i<posteriorMean.Count(); i++)
            {
                posteriorMeanToPlot[i] = posteriorMean[i];
                posterior95CIToPlot[i] = posterior95CI[i];
            }
        }
        else
        {
            posteriorMeanToPlot = posteriorMean;
            posterior95CIToPlot = posterior95CI;
        }

        string[] groupNames = Enumerable.Range(0, maxNCoefs).Select(d => d.ToString()).ToArray();

		string[] seriesNames = { "true", "estimated" };

        avaPlot2.Plot.Clear();

        double[][] ys_tmp = { coefsToPlot, posteriorMeanToPlot };
        double[][] yErr_tmp = { coefs95PCIToPlot, posterior95CIToPlot };
		avaPlot2.Plot.PlotBarGroups(
    		groupLabels: groupNames,
    		seriesLabels: seriesNames,
    		ys: ys_tmp,
    		yErr: yErr_tmp);
        avaPlot2.Plot.XLabel("Index");
        avaPlot2.Plot.YLabel("Coefficient");

		// customize the plot to make it look nicer
        avaPlot2.Plot.XAxis.Grid(false); // Disable vertical grid lines
        avaPlot2.Plot.YAxis.Grid(true);
		avaPlot2.Plot.Legend(location: Alignment.UpperCenter);

        avaPlot2.Refresh();
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }
}

