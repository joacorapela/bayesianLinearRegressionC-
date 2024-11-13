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


public class PredictionsVsResponsesVis: IObserver<(IList<(double, double)>, IList<double>)>
{
    private ScottPlot.Plottable.ScatterPlot _scatterPlot;

    private double[] _axisLimits;

    public AvaPlot window;

    public double[] axisLimits
    {
        set {
            this._axisLimits = value;
            this._scatterPlot = this.window.Plot.AddScatter(new double[] { value[0], value[1] }, new double[] { value[2], value[3] }, lineWidth: 0, color: Color.Red);
            this.window.Plot.XLabel("Observations");
            this.window.Plot.YLabel("Predicted Means");
            this.window.Plot.SetAxisLimits(value[0], value[1], value[2], value[3]);
            this.window.Refresh();
        }
        get {
            return _axisLimits;
        }
    }

    public void OnNext((IList<(double, double)>, IList<double>) pair)
    // pair.Item1.Item1: mean of predictions
    // pair.Item1.Item2: var of predictions
    // pair.Item2: observations
    {
        double[] observations = new double[pair.Item2.Count()];
        for (int i=0; i<observations.Length; i++)
        {
            observations[i] = pair.Item2[i];
        }
        // IList<double> observations = pair.Item2;
        double[] predicted_means = new double[pair.Item1.Count()];
        for (int i=0; i<predicted_means.Length; i++)
        {
            predicted_means[i] = pair.Item1[i].Item1;
        }
        this._scatterPlot.Update(observations, predicted_means);
		double corCoef = this.ComputeCoeff(observations, predicted_means);
        this.window.Plot.Title($"Correlation Coefficient: {corCoef:F2}");
        this.window.Refresh();
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

	public double ComputeCoeff(double[] values1, double[] values2)
	{
    	if(values1.Length != values2.Length)
        	throw new ArgumentException("values must be the same length");

    	var avg1 = values1.Average();
    	var avg2 = values2.Average();

    	var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

    	var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
    	var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

    	var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

    	return result;
	}

}


