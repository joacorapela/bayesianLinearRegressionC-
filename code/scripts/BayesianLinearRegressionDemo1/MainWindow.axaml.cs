
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.IO;
using Avalonia.Controls;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot.Avalonia;

namespace BayesianLinearRegressionDemo1;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var secondWindow = new SecondWindow();
        secondWindow.Show();

        double timeStartTimeSecs = 0.0;
        double timerPeriodSecs = 0.01;
        // double timerPeriodSecs = 1.00;
        IObservable<long> timer = Observable.Timer(TimeSpan.FromSeconds(timeStartTimeSecs), TimeSpan.FromSeconds(timerPeriodSecs));

        int modelSelectionBatchSize = 1000;
        // double sigma = 0.3;
        double sigma = 0.3;
        double priorPrecision = 2.0;

        double likePrecision = Math.Pow((1.0/sigma), 2);

		string coefsFolder = "./tmp";
		string coefsFilename = coefsFolder + "/" + "coefs.csv";

        // build single and batch observations data sources
        // Vector<double> coefs = new DenseVector(new[] { 0.5, 0.5, 0.5, 0.5 });
        int maxNCoefs = 10;
        List<Func<double, double>> basisFunctions = RegressionUtils.GetPolynomialBasisFunctions(maxNCoefs);
        RegressionObservationsDataSource xtDS = new RegressionObservationsDataSource();
        xtDS.sigma = sigma;
        xtDS.basisFunctions = basisFunctions;

        // build FileSystemObservable
    	IObservable<FileSystemEventArgs> fileSystemObservable = CreateFileSystemObservable(folder: coefsFolder);
        FileCoefsDataSource fileCoefsDS = new FileCoefsDataSource();
        IObservable<Vector<double>> coefsO = fileCoefsDS.Process(fileSystemObservable);

        IObservable<RegressionObservation> regressionObservationsO = xtDS.Process(timer, coefsO);
        IObservable<IList<RegressionObservation>> batchRegressionObservationsO = regressionObservationsO.Buffer(modelSelectionBatchSize);

        // model selection
        LinearRegressionMarginalLikeModelOrderSelector modelSelector = new LinearRegressionMarginalLikeModelOrderSelector();
        modelSelector.priorPrecision = priorPrecision;
        modelSelector.likelihoodPrecision = likePrecision;
        modelSelector.basisFunctions = basisFunctions;
        IObservable<int> numCoefsO = modelSelector.Process(batchRegressionObservationsO);

        ModelSelectorVisualizer modelSlectorV = new ModelSelectorVisualizer();
        numCoefsO.Subscribe(modelSlectorV);

        // coef calculator
        double[] m0 = new[] { 0.0, 0.0, 0.0, 0.0 };
        double[,] S0 = new[,] { {1.0, 0.0, 0.0, 0.0}, {0.0, 1.0, 0.0, 0.0}, {0.0, 0.0, 1.0, 0.0}, {0.0, 0.0, 0.0, 1.0} };
        PosteriorCalculator posteriorCalculator = new PosteriorCalculator();
        posteriorCalculator.priorPrecision = priorPrecision;
        posteriorCalculator.likelihoodPrecision = likePrecision;
        posteriorCalculator.m0 = m0;
        posteriorCalculator.S0 = S0;
        IObservable<PosteriorDataItem> posDataItemO = posteriorCalculator.Process(regressionObservationsO, numCoefsO);

        // PosteriorVisualizer posV = new PosteriorVisualizer();
        // posDataItemO.Subscribe(posV);

        // marginal ll calculator
        MarginalLLCalculator mllCalc = new MarginalLLCalculator();
        mllCalc.basisFunctions = basisFunctions;
        mllCalc.priorPrecision = priorPrecision;
        mllCalc.likelihoodPrecision = likePrecision;
        IObservable<double> mllO = mllCalc.Process(batchRegressionObservationsO, posDataItemO);

        MLLvisualizer mllV = new MLLvisualizer();
        mllO.Subscribe(mllV);

        // predictions calculator
        // IndependentVariableExtractor xExtractor = new IndependentVariableExtractor();
        // IObservable<PredictionObservation> xO = xExtractor.Process(regressionObservationsO);
        // PredictionsCalculator predCalc = new PredictionsCalculator();
        // predCalc.basisFunctions = basisFunctions;
        // IObservable<PredictionDataItem> predDataItemO = predCalculator.Process(xO, posDataItemO);

        // visualize predictions
        int visualizationBatchSize = 200;
        IObservable<IList<RegressionObservation>> batchRegressionObservationsO2 = regressionObservationsO.Buffer(visualizationBatchSize);
        IObservable<BatchRegressionObsAndPosterior> batchROandPosteriorsO = batchRegressionObservationsO2.WithLatestFrom(posDataItemO,
            (batchRObs, pdi) =>
            {
                // Console.WriteLine($"CombineLatest called. Length batchRObs: {batchRObs.Count}");
                var answer = new BatchRegressionObsAndPosterior();
                answer.batchRObs= (System.Collections.Generic.List<RegressionObservation>) batchRObs;
                answer.pdi = pdi;
                return answer;
            });
        BatchRegressionObsAndPosteriorsVis batchROandPosVis = new BatchRegressionObsAndPosteriorsVis();
        batchROandPosVis.beta = likePrecision;
        batchROandPosVis.basisFunctions = basisFunctions;
        batchROandPosVis.avaPlot1 = this.Find<AvaPlot>("AvaPlot1");
        batchROandPosteriorsO.Subscribe(batchROandPosVis);

        // visualize coefs
        IObservable<CoefsAndPosteriorDataItem> coefAndPosteriorDIO = coefsO.CombineLatest(posDataItemO,
            (coefs, pdi) =>
            {
                var answer = new CoefsAndPosteriorDataItem();
                answer.coefs = coefs;
                answer.pdi = pdi;
                return answer;
            });
        CoefsAndPosteriorVis coefsAndPostVis = new CoefsAndPosteriorVis();
        coefsAndPostVis.avaPlot2 = secondWindow.Find<AvaPlot>("AvaPlot2");
        coefAndPosteriorDIO.Subscribe(coefsAndPostVis);
    }

    IObservable<FileSystemEventArgs> CreateFileSystemObservable(string folder)
    {
        return
            // Observable.Defer enables us to avoid doing any work
            // until we have a subscriber.
            Observable.Defer(() =>
                {
                    FileSystemWatcher fsw = new(folder);
                    fsw.EnableRaisingEvents = true;

                    return Observable.Return(fsw);
                })
            // Once the preceding part emits the FileSystemWatcher
            // (which will happen when someone first subscribes), we
            // want to wrap all the events as IObservable<T>s, for which
            // we'll use a projection. To avoid ending up with an
            // IObservable<IObservable<FileSystemEventArgs>>, we use
            // SelectMany, which effectively flattens it by one level.
            .SelectMany(fsw =>
                Observable.Merge(new[]
                    {
                        Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                            h => fsw.Created += h, h => fsw.Created -= h),
                        Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                            h => fsw.Changed += h, h => fsw.Changed -= h),
                        Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(
                            h => fsw.Renamed += h, h => fsw.Renamed -= h),
                        Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                            h => fsw.Deleted += h, h => fsw.Deleted -= h)
                    })
                // FromEventPattern supplies both the sender and the event
                // args. Extract just the latter.
                .Select(ep => ep.EventArgs)
                // The Finally here ensures the watcher gets shut down once
                // we have no subscribers.
                .Finally(() => fsw.Dispose()))
            // This combination of Publish and RefCount means that multiple
            // subscribers will get to share a single FileSystemWatcher,
            // but that it gets shut down if all subscribers unsubscribe.
            .Publish()
            .RefCount();
    }
}
