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


public class PredictionsVsResponsesVis
{
    private int _numPointsToSimDisplay;
    private double[] _observations;
    private double[] _predictions;
    private ScottPlot.Plottable.ScatterPlot _scatterPlot;

    public AvaPlot avaPlot;

    public int numPointsToSimDisplay
    {
        set {
            this._numPointsToSimDisplay = value;
            this._observations = new double[this._numPointsToSimDisplay];
            this._predictions = new double[this._numPointsToSimDisplay];
            this.avaPlot.Plot.AddScatter(new double[] { -4.0, 4.0 }, new double[] { -4.0, 4.0 }, lineWidth: 1, color: Color.Red);
            this._scatterPlot = this.avaPlot.Plot.AddScatter(this._observations, this._predictions, lineWidth: 0, color: Color.Blue);
            this.avaPlot.Plot.XLabel("Observations");
            this.avaPlot.Plot.YLabel("Predictions");
            this.avaPlot.Plot.SetAxisLimits(-4.0, 4.0, -4.0, 4.0);
            this.avaPlot.Refresh();
        }
        get { return this._numPointsToSimDisplay; }
    }


    public IObservable<((double, double), double)> Process(IObservable<((double, double), double)> source)
    // source.Item1.Item1: mean of predictions
    // source.Item1.Item2: var of predictions
    // source.Item2: observations
    {
        source.Subscribe(pair =>
        {
            Array.Copy(this._observations, 1, this._observations, 0, this._observations.Length - 1);
            Array.Copy(this._predictions, 1, this._predictions, 0, this._predictions.Length - 1);
            this._observations[^1] = pair.Item1.Item1;
            this._predictions[^1] = pair.Item2;
            this._scatterPlot.Update(this._observations, this._predictions);
            this.avaPlot.Refresh();
        });
        return source;
    }

}
