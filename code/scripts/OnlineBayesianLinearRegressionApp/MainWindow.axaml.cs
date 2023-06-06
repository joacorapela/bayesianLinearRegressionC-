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


namespace OnlineBayesianLinearRegressionApp;

public partial class MainWindow : Avalonia.Controls.Window
{
    private static Button _button;
    private static Plot _plt;
    private static Heatmap _hm;
    private static double[,] _buffer;
    private static double[] _x;
    private static double[] _y;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void button_Click(object sender, RoutedEventArgs e)
    {
        // Change button text when button is clicked.
        _button = (Button)sender;
        if (_button.Content.Equals("Run"))
        {
            _button.Content = "Running ...";
            _x = MathNet.Numerics.Generate.LinearRange(-1.0, 0.01, 1.0);
            _y = MathNet.Numerics.Generate.LinearRange(-1.0, 0.01, 1.0);
            _buffer = new double[_x.Length, _y.Length];

            _plt = new ScottPlot.Plot(400, 400);
            _hm = _plt.AddHeatmap(_buffer, lockScales: false);
            _hm.FlipVertically = true;
            _hm.XMin = -1.0;
            _hm.XMax = 1.0;
            _hm.YMin = -1.0;
            _hm.YMax = 1.0;
            var window = new ScottPlot.Avalonia.AvaPlotViewer(_plt);
            window.Show();
            // plt.Render();

            Thread t = new Thread(new ThreadStart(runOnlineBayesianLinearRegression));
            t.Start();
        }
        else if (_button.Content.Equals("Done"))
        {
            Environment.Exit(0);
        }
        else
        {
            string msg = String.Format("Invalid button.Content: {0}", _button.Content);
            throw new InvalidOperationException(msg);
        }
    }

    public static void update_window() {
        _hm.Update(_buffer);
        // _plt.Render();
    }

    public static void set_done() {
        _button.Content = "Done";
        // _plt.Render();
    }

    private static void runOnlineBayesianLinearRegression()
    {
        Console.WriteLine("Running online Bayesian linear regression");

        int n_samples = 20;
        double prior_precision_coef = 2.0;
        string data_filename_pattern = "../../../data/linearRegression_nSamples{0:D2}.xml";

        string data_filename = String.Format(data_filename_pattern, n_samples);

        XmlSerializer serializer = new XmlSerializer(typeof(Result));
        FileStream fs = new FileStream(data_filename, FileMode.Open);
        Result result = (Result) serializer.Deserialize(fs);

        Vector<double> independent_var = Vector<double>.Build.Dense(result.x);
        Vector<double> dependent_var = Vector<double>.Build.Dense(result.t);
        double a0 = result.a0;
        double a1 = result.a1;
        double sigma = result.sigma;
        double likelihood_precision_coef = Math.Pow((1.0/sigma), 2);

        Console.WriteLine(String.Format("a0={0}, a1={1}, sigma={2}", a0, a1, sigma));

        _plt.AddPoint(a0, a1, Color.Red, 10);

        Matrix<double> Phi = Matrix<double>.Build.Dense(n_samples, 1, (i,j) => 1.0);
        Phi = Phi.Append(independent_var.ToColumnMatrix());
        double alpha = prior_precision_coef;
        double beta = likelihood_precision_coef;

        // set prior
        double[] aux = {0.0, -0.0};
        Vector<double> m0 = Vector<double>.Build.DenseOfArray(aux);
        Matrix<double> S0 = 1.0 / alpha * Matrix<double>.Build.DenseIdentity(2);

        Vector<double> mn = m0;
        Matrix<double> Sn = S0;

        computeMultivariateGaussianPDForGrid(_buffer, mn, Sn);
        update_window();
        for (int n = 0; n < n_samples; n++) 
        {
            Console.WriteLine("Continue (y/n)?");
            String cont = Console.ReadLine();
            while (!(cont.Equals("y") ^ cont.Equals("n")))
            {
                Console.WriteLine("Invalid answer. Please type y/n");
                cont = Console.ReadLine();
            }
            if (cont.Equals("n"))
            {
                break;
            }

            double y = dependent_var[n];
            Vector<double> phi = Phi.Row(n);
            Console.WriteLine(String.Format("Processing {0} ({1})", n, n_samples));
            var res = BayesianLinearRegression.OnlineUpdate(mn, Sn, phi, y, alpha, beta);
            mn = res.mean;
            Sn = res.cov;
            Console.WriteLine(mn.ToString());
            Console.WriteLine(Sn.ToString());
            computeMultivariateGaussianPDForGrid(_buffer, mn, Sn);
            update_window();
        }
        set_done();
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
            }
        }
    }
}
