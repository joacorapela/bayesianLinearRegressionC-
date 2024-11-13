
using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot.Avalonia;

namespace RFsNaturalVsRandomStimSimpleCell;

public partial class MainWindow : Window
{
    private (double[], double[,]) GetDelayedResponsesAndPhis(int delay, double[,] uResponses, double[,] uImages)
    {

        double[] responses = new double[uResponses.GetLength(0) - delay];
        for (int i=delay; i<responses.Length; i++)
        {
            responses[i-delay] = uResponses[i, 0];
        }

        // add column of ones to uImages and delay them, yielding matrix phis

        	// Dimensions of the original matrix
        int rows = uImages.GetLength(0);
        int cols = uImages.GetLength(1);

        	// Create a new matrix with an additional column (3x4 matrix in this example)
        // var phis = Matrix<double>.Build.Dense(rows, cols + 1);
        var phis = new double[rows-delay, cols + 1];

        	// Set the first column to ones
        for (int i = 0; i < rows-delay; i++)
        {
            phis[i, 0] = 1.0;
        }

        	// Copy the original matrix into the new matrix, starting from the second column
        // phis.SetSubMatrix(0, 0, 0, 1, uImages);
        for (int i = 0; i < rows-delay; i++)
        {
			for (int j = 0; j < cols; j++)
			{
            	phis[i, j+1] = uImages[i, j];
			}
        }

        return (responses, phis);
    }

    public MainWindow()
    {
        InitializeComponent();

        // define constants
        double timeStartTimeSecs = 0.0;
        double timer1PeriodSecs = 0.01;
        double likePrecision = 1.0;
        double testProportion = 1.0/6;
        int nForDisplay = 100; // display RF, coefs and predictions once time every nForDisplay samples

        // uImages stands for unDelayedImages
        // uResponses stands for unDelayedResponses

        // define delay between responses and images (in samples)
        // i.e., a cell response may be more related to an image presented in the past
        int delay = 1;

        double[] axisLimits = [0.0, 15.0, 0.0, 2.75];
        // get uImages and uResponses
        double priorPrecision = 1500.0;
        double[,] uImages = CSVReader.ReadCSVToMatrix("data/equalpower_C2_25hzPP.dat", " ");
        double[,] uResponses = CSVReader.ReadCSVToMatrix("data/nsSumSpikeRates.dat");
        // double priorPrecision = 10.0;
        // double[,] uImages = CSVReader.ReadCSVToMatrix("data/rsImagesC2PP.dat", " ");
        // double[,] uResponses = CSVReader.ReadCSVToMatrix("data/rsSumSpikeRates.dat");

        var aux = GetDelayedResponsesAndPhis(delay, uResponses, uImages);
        double[] responses = aux.Item1;
        double[,] phis = aux.Item2;

        IObservable<long> timer = Observable.Timer(TimeSpan.FromSeconds(timeStartTimeSecs), TimeSpan.FromSeconds(timer1PeriodSecs));

        // build regression observations
        VisualCellResponsesDataSource vcrDs = new VisualCellResponsesDataSource();
        vcrDs.images = Matrix<double>.Build.DenseOfArray(phis);
        vcrDs.responses = Vector<double>.Build.DenseOfArray(responses);

        int testSize = (int) (testProportion * responses.Length);
        Console.WriteLine($"testSize={testSize}");

        IObservable<RegressionObservation> regObs = vcrDs.Process(timer);

        IObservable<RegressionObservation> trainRegObsO = regObs.Skip(testSize);
        IObservable<IList<RegressionObservation>> testBatchRegObs = regObs.Take(testSize).Buffer(testSize);

        double[] m0 = new double[uImages.GetLength(1)+1];
        double[,] S0 = ((1.0 / priorPrecision) * (Matrix<double>.Build.DenseIdentity(uImages.GetLength(1)+1))).ToArray();

        PosteriorCalculator posteriorCalculator = new PosteriorCalculator();
        posteriorCalculator.priorPrecision = priorPrecision;
        posteriorCalculator.likePrecision = likePrecision;
        posteriorCalculator.m0 = m0;
        posteriorCalculator.S0 = S0;
        IObservable<PosteriorDataItem> fullPosDataItemO = posteriorCalculator.Process(trainRegObsO);
        IObservable<PosteriorDataItem> posDataItemO = fullPosDataItemO.Where((value, index) => (index + 1) % nForDisplay == 0);

        // combine each batchRegObs with the latest posDataItem
        IObservable<(PosteriorDataItem, IList<RegressionObservation>)> auxO = posDataItemO.WithLatestFrom(testBatchRegObs, (pdi, batchRegObs) => (pdi, batchRegObs));
        IObservable<(PosteriorDataItem, IList<RegressionObservation>)> pdiAndBatchRegObsO =  auxO.Publish().RefCount();

        // select pdis and IList<phis> from pdiAndBatchRegObsO
        IObservable<(PosteriorDataItem, IList<Vector<double>>)> pdiAndBatchPhisO = pdiAndBatchRegObsO.Select(
            pdiAndBatchRegObs =>
            {
                IList<RegressionObservation> regObservations = pdiAndBatchRegObs.Item2;
                IList<Vector<double>> batchPhis = new List<Vector<double>>();
                foreach (RegressionObservation regObservation in regObservations)
                {
                    batchPhis.Add(regObservation.phi);
                }
                return (pdiAndBatchRegObs.Item1, batchPhis);
            });

        // select IList<ts> from pdiAndBatchRegObsO
        IObservable<IList<double>> batchTsO = pdiAndBatchRegObsO.Select(pdiAndBatchRegObs =>
        {
            IList<RegressionObservation> regObservations = pdiAndBatchRegObs.Item2;
            IList<double> batchTs = new List<double>();
            foreach (RegressionObservation regObservation in regObservations)
            {
                batchTs.Add(regObservation.t);
            }
            return (batchTs);
        });

        // computer predictions
        PredictionsCalculator predCalc = new PredictionsCalculator();
        IObservable<IList<(double, double)>> batchPredictionsO = predCalc.Process(pdiAndBatchPhisO);

        // zip predictions with true responses
        IObservable<(IList<(double, double)>, IList<double>)>  batchPredAndTrueRespO = batchPredictionsO.Zip(batchTsO, (batchPred, batchTs) => (batchPred, batchTs));

		// visualize RFs
        ReceptiveFieldVisualiser rfVis = new ReceptiveFieldVisualiser();
        rfVis.window = this.Find<AvaPlot>("RFsWindowAvaPlot");
        posDataItemO.Subscribe(rfVis);

        // visualize coefs
        var coefsWin = new CoefsWindow();
        coefsWin.Show();

        CoefsVisualiser coefsVisualiser = new CoefsVisualiser();
        coefsVisualiser.window = coefsWin.Find<AvaPlot>("CoefsWindowAvaPlot");
        posDataItemO.Subscribe(coefsVisualiser);

        // visualize predictions and resposes
        var predictionsWin = new PredictionsWindow();
        predictionsWin.Show();

        PredictionsVsResponsesVis predVsRespVis = new PredictionsVsResponsesVis();
        predVsRespVis.window = predictionsWin.Find<AvaPlot>("PredictionsWindowAvaPlot");
        predVsRespVis.axisLimits = axisLimits;
        batchPredAndTrueRespO.Subscribe(predVsRespVis);
    }
}
