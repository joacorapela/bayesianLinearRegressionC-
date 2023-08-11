using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace JoacoRapela.Statistics
{
    [Combinator]
    [Description("")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class BayesianLinearRegression
    {
        public Vector<double> M0 { get; set; }

        public Matrix<double> S0 { get; set; }

        public double Alpha { get; set; }

        public double Beta { get; set; }

        public static (Vector<double> mean, Matrix<double> cov) OnlineUpdate(Vector<double> mn, Matrix<double> Sn, Vector<double> phi, double y, double alpha, double beta)
        {
            Vector<double> aux1 = Sn.Multiply(phi);
            double aux2 = 1.0/(1.0 / beta + phi.ToRowMatrix().Multiply(Sn).Multiply(phi)[0]);

            Matrix<double> Snp1 = Sn - aux2 * aux1.OuterProduct(aux1);
            Vector<double> mnp1 = beta * y * Snp1.Multiply(phi) + mn - aux2 * phi.DotProduct(mn) * Sn.Multiply(phi);

            return (mnp1, Snp1);
        }
        
        public IObservable<PosteriorDataItem> Process(IObservable<RegressionObservation> source)
        {
            return Observable.Defer(() =>
            {
                Vector<double> mn = M0;
                Matrix<double> Sn = S0;
                double alpha = Alpha;
                double beta = Beta;
                return source.Select(observation =>
                {
                    double x = observation.x;
                    double t = observation.t;
                    double[] aux = new[] {1, x};
                    Vector<double> phi = Vector<double>.Build.DenseOfArray(aux);
                    (mn, Sn) = BayesianLinearRegression.OnlineUpdate(mn, Sn, phi, t, alpha, beta);
                    return new PosteriorDataItem { mn = mn, Sn = Sn };
                });
            });
        }
    }
}
