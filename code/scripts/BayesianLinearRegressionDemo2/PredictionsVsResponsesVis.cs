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


public class PredictionsVsResponsesVis : IObserver<((double[], double[]), double[])>
{
    public AvaPlot avaPlot1;
    public double beta;

    public void OnNext(((double[], double[]), double[])  predictionsAndResponses)
    {
        Console.WriteLine("PredictionsVsResponsesVis::OnNext called");
        (double[], double[]) predictions = predictionsAndResponses.Item1;
        double[] responses = predictionsAndResponses.Item2;

        double[] means = predictions.Item1;

        // plot means and 95% ci for xDense
        avaPlot1.Plot.Clear();
        avaPlot1.Plot.AddScatter(responses, means, Color.Blue, lineWidth: 0);
        // avaPlot1.Plot.AddFillError(t, mean, ci95Width, Color.FromArgb(50, Color.Blue));

        // plot data
        avaPlot1.Plot.AddScatter(responses, responses, Color.Red, markerSize: 0);
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

