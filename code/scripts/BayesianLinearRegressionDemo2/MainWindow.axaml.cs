
using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot.Avalonia;

namespace BayesianLinearRegressionDemo2;

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
        IObservable<(RegressionObservation, PosteriorDataItem)> aux = regObsO2.WithLatestFrom(posDataItemO, (regObs, pdi) => (regObs, pdi));
        IObservable<(RegressionObservation, PosteriorDataItem)> regObs2AndPDIO =  aux.Publish().RefCount();

        // select phis and pdis from regObs2AndPDIO
        IObservable<(Vector<double>, PosteriorDataItem)> phisAndPDIsO = regObs2AndPDIO.Select(regObs2AndPDI => (regObs2AndPDI.Item1.phi, regObs2AndPDI.Item2));

        // select ts from regObs2AndPDIO
        IObservable<double> tsO = regObs2AndPDIO.Select(regObs2AndPDI => regObs2AndPDI.Item1.t);

        // tsO.Subscribe(t => Console.WriteLine($"t: {t}"));

        // computer predictions
        PredictionsCalculator predCalc = new PredictionsCalculator();
        IObservable<(double, double)> predictionsO = predCalc.Process(phisAndPDIsO);

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
