
using System.Reactive.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using MathNet.Numerics.LinearAlgebra;

namespace BayesianLinearRegressionDemo2;

using System;
using System.IO;

public class CSVReader
{
    public static double[] ReadCSVToVector(string filename)
    {
        // Read all lines from the CSV file
        string[] lines = File.ReadAllLines(filename);
        string[] values_str = lines[0].Split(',');

        // Determine the dimensions of the matrix
        int numElem = values_str.Length;

        // Create the double vector
        double[] values_double = new double[numElem];

        // Fill the matrix with the data from the CSV
        for (int i = 0; i < numElem; i++)
        {
            // Attempt to parse each value to double
            if (double.TryParse(values_str[i], out double result))
            {
                values_double[i] = result;
            }
            else
            {
                throw new FormatException($"Unable to parse '{values_str[i]}' as a double at element {i}.");
            }
        }

        return values_double;
    }
}

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var coefsWin = new CoefsWindow();
        coefsWin.Show();

        var rfsWin = new RFsWindow();
        rfsWin.Show();

        double timeStartTimeSecs = 0.0;
        double timer1PeriodSecs = 1.0;
        double timer2PeriodSecs = 1.5;
        IObservable<long> timer1 = Observable.Timer(TimeSpan.FromSeconds(timeStartTimeSecs), TimeSpan.FromSeconds(timer1PeriodSecs));
        IObservable<long> timer2 = Observable.Timer(TimeSpan.FromSeconds(timeStartTimeSecs), TimeSpan.FromSeconds(timer2PeriodSecs));

        double sigma = 0.3;
        double priorPrecision = 2.0;

        double likePrecision = Math.Pow((1.0/sigma), 2);

        // build regression observations
        double[] tmp = CSVReader.ReadCSVToVector(@"C:\Users\user1\bonsai\repos\bayesianLinearRegressionCSharp\code\scripts\BayesianLinearRegressionDemo2\data\gabor10x10.csv");
        Vector<double> coefs = Vector<double>.Build.DenseOfArray(tmp);

        VisualCellResponsesDataSource vcrDs = new VisualCellResponsesDataSource(); // visual cell responses observable
        vcrDs.sigma = sigma;
        vcrDs.coefs = coefs;

        IObservable<RegressionObservation> regObsO1 = vcrDs.Process(timer1);
        IObservable<RegressionObservation> regObsO2 = vcrDs.Process(timer2);

        // run online Bayesian linear regression
        double[] m0 = new double[coefs.Count];

        double[,] S0 = Matrix<double>.Build.DenseIdentity(coefs.Count).ToArray();
        PosteriorCalculator posteriorCalculator = new PosteriorCalculator();
        posteriorCalculator.priorPrecision = priorPrecision;
        posteriorCalculator.likePrecision = likePrecision;
        posteriorCalculator.m0 = m0;
        posteriorCalculator.S0 = S0;
        IObservable<PosteriorDataItem> posDataItemO = posteriorCalculator.Process(regObsO1);

        // buffer regression observations
        int visualizationBatchSize = 20;
        var batchRegObsO = regObsO2.Buffer(visualizationBatchSize);

        // split buffer of regression observations into a list of stimuli and a list of responses
        IObservable<List<Vector<double>>> batchPhisO = batchRegObsO.Select(listRegObs => 
        {
            List<Vector<double>> phisList = new List<Vector<double>>();
            foreach (RegressionObservation regObs in listRegObs)
            {
                phisList.Add(regObs.phi);
            }
            return phisList;
        });
        IObservable<List<double>> batchTsO = batchRegObsO.Select(listRegObs => 
        {
            List<double> tsList = new List<double>();
            foreach (RegressionObservation regObs in listRegObs)
            {
                tsList.Add(regObs.t);
            }
            return tsList;
        });

        // combine each batchPhisO with the latest posDataItem
        IObservable<(List<Vector<double>>, PosteriorDataItem)> batchPhisAndPostDataItemO = batchPhisO.WithLatestFrom(posDataItemO, (batchPhis, pdi) => (batchPhis, pdi));
        Console.WriteLine("Stop here");
        // BatchPredictionsCalculator bPredCalc = new BatchPredictionsCalculator();
        // bPredCal.beta = likePrecision;
        // var predictionsO = bPredCalc.Process(batchPhisAndPostDataItemO);

        // zip predictions with true responses
        // var predAndTrueRespO = predictionsO.zip(tsO, (pred, t) => (pred, t));

        // visualize predictions and resposes
        // PredictionsVsResponsesVis predVsRespVis = new PredictionsVsResponsesVis();
        // predVsRespVis.beta = likePrecision;
        // predVsRespVis.avaPlot1 = this.Find<AvaPlot>("PredictionsAvaPlot");
        // predAndTrueRespO.Subscribe(predVsRespVis);

        // visualize coefs
        // CoefsAndPosteriorVis coefsAndPostVis = new CoefsAndPosteriorVis();
        // coefsAndPostVis.coefs = coefs.ToArray();
        // coefsAndPostVis.window = coefsWin.Find<AvaPlot>("CoefsWindowAvaPlot");
        // posDataItemO.Subscribe(coefsAndPostVis);

        // true and estimated RFs visualizer
        // TrueAndEstimatedRFsVis trueAndEstRFsVis = new TrueAndEstimatedRFsVis();
        // trueAndEstRFsVis.coefs = coefs.ToArray();
        // trueAndEstRFsVis.window = rfsWin.Find<AvaPlot>("RFsWindowAvaPlot");
        // posDataItemO.Subscribe(trueAndEstRFsVis);
    }
}
