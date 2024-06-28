
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class PosteriorCalculator
{
    public double[] m0
    {
        get;
        set;
    }

    public double[,] S0
    {
        get;
        set;
    }

    public double[] n0
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

    public IObservable<PosteriorDataItem> Process(IObservable<RegressionObservation> observations,
                                                  IObservable<int> numBasisFunctions)
    {
        Console.WriteLine("PosteriorCalculator Process called");
        IObservable<RegressionObservationAndNumber> observationsAndNums = observations.CombineLatest(numBasisFunctions,
            (observation, num) =>
            {
                RegressionObservationAndNumber obsAndNum = new RegressionObservationAndNumber();
                obsAndNum.o = observation;
                obsAndNum.n = num;
                return obsAndNum;
            }
        );
        return observationsAndNums.Scan(
            new PosteriorDataItem
            {
                mn = Vector<double>.Build.DenseOfArray(m0),
                Sn = Matrix<double>.Build.DenseOfArray(S0)
            },
            (prior, obsAndNum) =>
            {
                // Console.WriteLine("PosteriorCalculator generating a new PosteriorDataItem");
                RegressionObservation obs = obsAndNum.o;
                int numBasis = obsAndNum.n + 1;
                List<Func<double, double>> basisFunctions = RegressionUtils.GetPolynomialBasisFunctions(M: numBasis-1);
                Vector<double> phi = RegressionUtils.BuildDesignMatrixRow(obs.x, basisFunctions);
                if (prior.mn.Count < numBasis)
                {
                    prior.mn  = MathNETutils.AddElementsToVector(prior.mn, numBasis - prior.mn.Count, 0.0);
                    prior.Sn = MathNETutils.AddRowsAndColsToSquareMatrix(prior.Sn, numBasis - prior.Sn.RowCount, 1.0, 0.0);
                }
                else if (prior.mn.Count > numBasis)
                {
                    int nElemToRemove = prior.mn.Count-numBasis;
                    List<int> indicesToRemove = new List<int>();
                    for (int i=0; i<nElemToRemove; i++)
                    {
                        indicesToRemove.Add(prior.mn.Count - 1 - i);
                    }
                    prior.mn  = MathNETutils.RemoveElementsFromVector(prior.mn, indicesToRemove);
                    prior.Sn = MathNETutils.RemoveRowsAndColsFromSquareMatrix(prior.Sn, indicesToRemove);
                }
                var post = BayesianLinearRegression.OnlineUpdate(prior.mn, prior.Sn, phi, obs.t, priorPrecision, likelihoodPrecision);
                PosteriorDataItem pdi = new PosteriorDataItem { mn = post.mean, Sn = post.cov };
                return pdi;
            });
    }
}
