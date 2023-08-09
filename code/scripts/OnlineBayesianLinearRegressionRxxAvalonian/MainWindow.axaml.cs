using ScottPlot;
using ScottPlot.Plottable;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;


namespace OnlineBayesianLinearRegressionRxxWithGUI;

public partial class MainWindow : Avalonia.Controls.Window, IObserver<PosteriorDataItem>
{
    private static Heatmap _hm;

    private static RegressionObservationsDataSource _data_source;
    private static double[,] _buffer;
    private static double[] _x;
    private static double[] _y;

    public MainWindow()
    {
        InitializeComponent();

        double sigma = 0.3;
        double prior_precision_coef = 2.0;

        System.Random rng = SystemRandomSource.Default;
        double a0 = (2*rng.NextDouble()-1)*0.7;
        double a1 = (2*rng.NextDouble()-1)*0.7;
        _x = MathNet.Numerics.Generate.LinearRange(-1.0, 0.01, 1.0);
        _y = MathNet.Numerics.Generate.LinearRange(-1.0, 0.01, 1.0);
        _buffer = new double[_x.Length, _y.Length];

        Plot plt = new ScottPlot.Plot(400, 400);
        _hm = plt.AddHeatmap(_buffer, lockScales: false);
        _hm.FlipVertically = true;
        _hm.XMin = -1.0;
        _hm.XMax = 1.0;
        _hm.YMin = -1.0;
        _hm.YMax = 1.0;

        double likelihood_precision_coef = Math.Pow((1.0/sigma), 2);

        _data_source = new RegressionObservationsDataSource(a0=a0, a1=a1, sigma=sigma);

        double[] aux = {0.0, -0.0};
        Vector<double> m0 = Vector<double>.Build.DenseOfArray(aux);
        Matrix<double> S0 = 1.0 / prior_precision_coef * Matrix<double>.Build.DenseIdentity(2);

        OnlineBayesianLinearRegression oblr = new OnlineBayesianLinearRegression(prior_precision_coef, likelihood_precision_coef, m0, S0);

        _data_source.Subscribe(oblr);
        oblr.Subscribe(this);

        plt.AddPoint(a0, a1, Color.Red, 10);
        computeMultivariateGaussianPDForGrid(_buffer, m0, S0);
        _hm.Update(_buffer);
        var window = new ScottPlot.Avalonia.AvaPlotViewer(plt);
        window.Show();
    }

    public void button_Click(object sender, RoutedEventArgs e)
    {
        // Change button text when button is clicked.
        Button button = (Button)sender;
        if (button.Content.Equals("Publish Observation"))
        {
            Console.WriteLine("Button Publish Observation pressed");
            _data_source.PublishNextObservation();
        }
        else if (button.Content.Equals("Exit"))
        {
            Console.WriteLine("Button Exit pressed");
            Environment.Exit(0);
        }
        else
        {
            string msg = String.Format("Invalid button.Content: {0}", button.Content);
            throw new InvalidOperationException(msg);
        }
    }

    public void OnNext(PosteriorDataItem data_item)
    {
        Console.WriteLine("MainWindow::OnNext called");
        computeMultivariateGaussianPDForGrid(_buffer, data_item.mn, data_item.Sn);
        _hm.Update(_buffer);
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnCompleted()
    {
    }

    private static void computeMultivariateGaussianPDForGrid(double[,] buffer, Vector<double> mn, Matrix<double> Sn)
    {
        double[] eval_loc_buffer = new double[2];
        MatrixNormal matrixNormal = new MatrixNormal(mn.ToColumnMatrix(), Sn, Matrix<double>.Build.DenseIdentity(1));
        for (int i = 0; i < _x.Length; i++)
        {
            eval_loc_buffer[0] = _x[i];
            for (int j = 0; j < _y.Length; j++)
            {
                eval_loc_buffer[1] = _y[j];
                Vector<double> eval_loc = Vector<double>.Build.Dense(eval_loc_buffer);
                buffer[j, i] = matrixNormal.Density(eval_loc.ToColumnMatrix());
                // Console.WriteLine(String.Format("buffer[{0},{1}]={2}", j, i, buffer[j, i]));
            }
        }
    }
}
