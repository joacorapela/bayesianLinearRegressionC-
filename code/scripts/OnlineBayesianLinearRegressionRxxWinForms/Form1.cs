
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

using ScottPlot;
using ScottPlot.Plottable;

using System;
using System.Reactive.Linq;

namespace OnlineBayesianLinearRegressionRxxWinForms
{
    public partial class Form1 : Form, IObserver<PosteriorDataItem>
    {
        readonly ScottPlot.FormsPlot _formsPlot1;
    	private static Heatmap _hm;
    	private static double[,] _buffer;
    	private static double[] _x;
    	private static double[] _y;

        public Form1()
        {
            InitializeComponent();

            // Add the FormsPlot
            _formsPlot1 = new() { Dock = DockStyle.Fill };
            Controls.Add(_formsPlot1);

            double sigma = 0.3;
            double priorPrecision = 2.0;

            System.Random rng = SystemRandomSource.Default;
            double a0 = (2*rng.NextDouble()-1)*0.7;
            double a1 = (2*rng.NextDouble()-1)*0.7;
            _x = MathNet.Numerics.Generate.LinearRange(-1.0, 0.01, 1.0);
            _y = MathNet.Numerics.Generate.LinearRange(-1.0, 0.01, 1.0);
            _buffer = new double[_x.Length, _y.Length];

            // Add sample data to the plot
            _hm = _formsPlot1.Plot.AddHeatmap(_buffer, lockScales: false);
            _hm.FlipVertically = true;
            _hm.XMin = -1.0;
            _hm.XMax = 1.0;
            _hm.YMin = -1.0;
            _hm.YMax = 1.0;
            // _formsPlot1.Frameless();
            _formsPlot1.Refresh();

            double likePrecision = Math.Pow((1.0/sigma), 2);

            double timeStartTimeSecs = 0.0;
            double timerPeriodSecs = 1.0;
            var timer = Observable.Timer(TimeSpan.FromSeconds(timeStartTimeSecs), TimeSpan.FromSeconds(timerPeriodSecs));

            RegressionObservationsDataSource dataSource = new RegressionObservationsDataSource();
            dataSource.a0 = a0;
            dataSource.a1 = a1;
            dataSource.sigma = sigma;

            IObservable<RegressionObservation> regressionObservations = dataSource.Process(timer);

            double[] m0 = {0.0, 0.0};
            double[,] S0 = { {1.0, 0.0}, {0.0, 1.0} };
            _formsPlot1.Plot.AddPoint(a0, a1, Color.Red, 10);
            computeMultivariateGaussianPDForGrid(_buffer, m0, S0);
            _hm.Update(_buffer);

            IObservable<PosteriorDataItem> postSeq = new PosteriorCalculator()
            {
                priorPrecision = priorPrecision,
                likePrecision = likePrecision,
                m0 = m0,
                S0 = S0
            }.Process(regressionObservations);
            postSeq.Subscribe(this);
        }

    	public void OnNext(PosteriorDataItem data_item)
    	{
            Console.WriteLine("MainWindow::OnNext called");
            computeMultivariateGaussianPDForGrid(_buffer, data_item.mn.ToArray(), data_item.Sn.ToArray());
            _hm.Update(_buffer);
            _formsPlot1.Refresh();
        }
    
        public void OnError(Exception error)
        {
            throw error;
        }
    
        public void OnCompleted()
        {
        }
    
        private static void computeMultivariateGaussianPDForGrid(double[,] buffer, double[] mn, double[,] Sn)
        {
            Vector<double> mnVec = Vector<double>.Build.DenseOfArray(mn);
            Matrix<double> SnMat = Matrix<double>.Build.DenseOfArray(Sn);
            double[] eval_loc_buffer = new double[2];
            MatrixNormal matrixNormal = new MatrixNormal(mnVec.ToColumnMatrix(), SnMat, Matrix<double>.Build.DenseIdentity(1));
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

}
