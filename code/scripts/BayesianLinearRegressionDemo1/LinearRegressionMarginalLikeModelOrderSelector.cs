
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra;

public class LinearRegressionMarginalLikeModelOrderSelector
{
    public List<Func<double, double>> basisFunctions
    {
        get;
        set;
    }

    public double priorPrecision
    {
        get;
        set;
    }

    public double likelihoodPrecision
    {
        get;
        set;
    }

    public IObservable<int> Process(IObservable<IList<RegressionObservation>> listsOfObservations)
    {
        Console.WriteLine("LinearRegressionMarginalLikeModelOrderSelector Process called");
        return listsOfObservations.Select(listOfObservations =>
        {
            Console.WriteLine("LinearRegressionMarginalLikeModelOrderSelector selecting model");
            int numObs = listOfObservations.Count;
            int maxNCol = basisFunctions.Count;
            Matrix<double> Phi = Matrix<double>.Build.Dense(numObs, maxNCol);
            Vector<double> y = Vector<double>.Build.Dense(numObs);

            // build Phi and y
            int rowIndex = 0;
            foreach (RegressionObservation o in listOfObservations)
            {
                Vector<double> row = RegressionUtils.BuildDesignMatrixRow(x: o.x, basisFunctions: basisFunctions);
                Phi.SetRow(rowIndex, row);
                y[rowIndex] = o.t;
                rowIndex += 1;
            }
            // DelimitedWriter.Write("/tmp/Phi.csv", Phi, ",");
            // DelimitedWriter.Write("/tmp/y.csv", y.ToColumnMatrix(), ",");
            // Console.WriteLine("Phi and y wrote to disk");
            // find best model order
            double maxLogE = double.MinValue;
            int bestM = -1;
            for (int nCols=1; nCols<=maxNCol; nCols++)
            {
                Matrix<double> PhiM = Phi.SubMatrix(0, numObs, 0, nCols);
                var res = BayesianLinearRegression.BatchWithSimplePrior(PhiM, y, priorPrecision, likelihoodPrecision);
                Vector<double> mN = res.mean;
                Matrix<double> SN = res.cov;
                double logE = BayesianLinearRegression.ComputeLogEvidence(PhiM, y, mN, SN, priorPrecision, likelihoodPrecision);
                if (logE > maxLogE)
                {
                    bestM = nCols - 1;
                    maxLogE = logE;
                }
            }
            return bestM;
        });
    }
}
