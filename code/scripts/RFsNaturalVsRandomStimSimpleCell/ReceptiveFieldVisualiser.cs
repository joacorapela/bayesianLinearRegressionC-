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

public class ReceptiveFieldVisualiser: IObserver<PosteriorDataItem>
{
    public AvaPlot window;

    private double[,] _toSquareMatrix(double[] value)
    {
        int matrixDim = Convert.ToInt32(Math.Sqrt(value.Count()));
        double[,] matrix = new double[matrixDim, matrixDim];
        int count = 0;
        for (int i=0; i<matrixDim; i++)
        {
            for (int j=0; j<matrixDim; j++)
            {
                matrix[i, j] = value[count];
                count++;
            }
        }
        return matrix;
    }

    public void OnNext(PosteriorDataItem pdi)
    {
		double[] rf = new double[pdi.mn.Count - 1];
		for (int i = 1; i < pdi.mn.Count; i++)
		{
			rf[i - 1] = pdi.mn.At(i);
		}
        Console.WriteLine("ReceptiveFieldVisualiser::OnNext called");
        this.window.Plot.Clear();
        var hm = this.window.Plot.AddHeatmap(this._toSquareMatrix(rf), lockScales: false);
        var cb = this.window.Plot.AddColorbar(hm);
        this.window.Plot.XLabel("x");
        this.window.Plot.YLabel("y");
        this.window.Refresh();
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }
}

