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

public class TrueAndEstimatedRFsVis: IObserver<PosteriorDataItem>
{
    private double[,] _coefs;

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

    public double[] coefs {
        set {this._coefs = this._toSquareMatrix(value);}
    }

    public AvaPlot window;

    public void OnNext(PosteriorDataItem pdi)
    {
        Console.WriteLine("TrueAndEstimatedRFsVis::OnNext called");
        window.Plot.Clear();
        // window.Plot.AddHeatmap(this._coefs);
        var hm = window.Plot.AddHeatmap(this._toSquareMatrix(pdi.mn.ToArray()), lockScales: false);
        var cb = window.Plot.AddColorbar(hm);
        // window.Plot.Frameless();
        // window.Plot.Margins(0, 0);
        // hm.Smooth = false;
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

