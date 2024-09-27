
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

namespace BayesianLinearRegressionDemo2;

using System;
using System.IO;
using System.Collections.Generic;

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
        double timerPeriodSecs = 1.0;
        IObservable<long> timer = Observable.Timer(TimeSpan.FromSeconds(timeStartTimeSecs), TimeSpan.FromSeconds(timerPeriodSecs));

        double sigma = 0.3;
        double priorPrecision = 2.0;

        double likePrecision = Math.Pow((1.0/sigma), 2);

        // build single and batch observations data sources
        double[] tmp = CSVReader.ReadCSVToVector("data/gabor10x10.csv");
		Vector<double> coefs = Vector<double>.Build.DenseOfArray(tmp);

        VisualCellResponsesDataSource vcrDs = new VisualCellResponsesDataSource(); // visual cell responses observable
        vcrDs.sigma = sigma;
        vcrDs.coefs = coefs;
        IObservable<RegressionObservation> regObsO = vcrDs.Process(timer);

        // posterior calculator
        double[] m0 = new double[coefs.Count];

        double[,] S0 = Matrix<double>.Build.DenseIdentity(coefs.Count).ToArray();
        PosteriorCalculator posteriorCalculator = new PosteriorCalculator();
        posteriorCalculator.priorPrecision = priorPrecision;
        posteriorCalculator.likePrecision = likePrecision;
        posteriorCalculator.m0 = m0;
        posteriorCalculator.S0 = S0;
        IObservable<PosteriorDataItem> posDataItemO = posteriorCalculator.Process(regObsO);

        // visualize predictions
        int visualizationBatchSize = 200;
        IObservable<IList<RegressionObservation>> bufferedRegObsO = regObsO.Buffer(visualizationBatchSize);
        var batchVCRsAndPosteriorsO = bufferedRegObsO.WithLatestFrom(posDataItemO,
            (bufferedRegObs, pdi) =>
            {
                (IList<RegressionObservation> bufferedRegObs, PosteriorDataItem pdi) answer = ((System.Collections.Generic.List<RegressionObservation>) bufferedRegObs, pdi);
                return answer;
            });
        PredictionsVsResponsesVis predVsRespVis = new PredictionsVsResponsesVis();
        predVsRespVis.beta = likePrecision;
        predVsRespVis.avaPlot1 = this.Find<AvaPlot>("PredictionsAvaPlot");
        batchVCRsAndPosteriorsO.Subscribe(predVsRespVis);

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
