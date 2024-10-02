
using System;
using System.IO;
using System.Reactive.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot.Avalonia;

namespace BayesianLinearRegressionDemo2;


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

        // double sigma = 0.3;
        double sigma = 1e-6;
        double priorPrecision = 1.0/sigma;
        double likePrecision = 1.0/sigma;

        // build regression observations
        double[] tmp = CSVReader.ReadCSVToVector("data/gabor10x10.csv");
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

        // combine each regObs2 with the latest posDataItem
        IObservable<(RegressionObservation, PosteriorDataItem)> regObsAndPDIO = regObsO2.WithLatestFrom(posDataItemO, (regObs, pdi) => (regObs, pdi));

        // select phis and pdis from regObsAndPDIO
        // IObservable<(PosteriorDataItem, Vector<double>)> phisAndPDIsO = regObsAndPDIO.Select((regObs, pdi) => (regObs.phi, pdi));
        var phisAndPDIsO = regObsAndPDIO.Select(regObsAndPDI => (regObsAndPDI.Item1.phi, regObsAndPDI.Item2));

        // select ts from regObsAndPDIO
        IObservable<double> tsO = regObsAndPDIO.Select(regObsAndPDI => regObsAndPDI.Item1.t);

        // computer predictions
        PredictionsCalculator predCalc = new PredictionsCalculator();
        // IObservable<(double, double)> predictionsO = predCalc.Process(phisAndPDIsO);
        var predictionsO = predCalc.Process(phisAndPDIsO);

        // zip predictions with true responses
        IObservable<((double, double), double)>  predAndTrueRespO = predictionsO.Zip(tsO, (pred, t) => (pred, t));

        // visualize predictions and resposes
        PredictionsVsResponsesVis predVsRespVis = new PredictionsVsResponsesVis();
        predVsRespVis.avaPlot = this.Find<AvaPlot>("PredictionsAvaPlot");
        predVsRespVis.numPointsToSimDisplay = 20;
        predVsRespVis.Process(predAndTrueRespO);

        // visualize coefs
        CoefsAndPosteriorVis coefsAndPostVis = new CoefsAndPosteriorVis();
        coefsAndPostVis.coefs = coefs.ToArray();
        coefsAndPostVis.window = coefsWin.Find<AvaPlot>("CoefsWindowAvaPlot");
        posDataItemO.Subscribe(coefsAndPostVis);

        // true and estimated RFs visualizer
        TrueAndEstimatedRFsVis trueAndEstRFsVis = new TrueAndEstimatedRFsVis();
        trueAndEstRFsVis.coefs = coefs.ToArray();
        trueAndEstRFsVis.window = rfsWin.Find<AvaPlot>("RFsWindowAvaPlot");
        posDataItemO.Subscribe(trueAndEstRFsVis);
    }
}
