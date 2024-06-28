
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

public class MarginalLLCalculator
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

    public IObservable<double> Process(IObservable<IList<RegressionObservation>> listsOfObservations, IObservable<PosteriorDataItem> posteriorO)
    {
        Console.WriteLine("MarginalLLCalculator Process called");
        IObservable<double> marginalLLO = listsOfObservations.WithLatestFrom(posteriorO,
            (listOfObs, postDataItem) =>
            {
                int numObs = listOfObs.Count;
                Vector<double> mN = postDataItem.mn;
                Matrix<double> SN = postDataItem.Sn;
                int nCoefs = mN.Count;
                Matrix<double> Phi = Matrix<double>.Build.Dense(numObs, nCoefs);
                Vector<double> y = Vector<double>.Build.Dense(numObs);

                // build Phi and y
                List<Func<double, double>> basisFunctionsM = new List<Func<double, double>>(basisFunctions.Take(nCoefs));
                int rowIndex = 1;
                foreach (RegressionObservation o in listOfObs)
                {
                    Vector<double> row = RegressionUtils.BuildDesignMatrixRow(o.x, basisFunctionsM);
                    Phi.SetRow(rowIndex, row);
                    y[rowIndex] = o.t;
                }

                // compute marginal LL
                double logE = BayesianLinearRegression.ComputeLogEvidence(Phi, y, mN, SN, priorPrecision, likelihoodPrecision);

                return logE;
            });
        return marginalLLO;
    }
}
