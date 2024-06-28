using System;
using MathNet.Numerics.LinearAlgebra;

public class BayesianLinearRegression
{
    static public (Vector<double> mean, Matrix<double> cov) OnlineUpdate(Vector<double> mn, Matrix<double> Sn, Vector<double> phi, double y, double alpha, double beta)
    {
         Vector<double> aux1 = Sn.Multiply(phi);
         double aux2 = 1.0/(1.0 / beta + phi.ToRowMatrix().Multiply(Sn).Multiply(phi)[0]);

         Matrix<double> Snp1 = Sn - aux2 * aux1.OuterProduct(aux1);
         Vector<double> mnp1 = beta * y * Snp1.Multiply(phi) + mn - aux2 * phi.DotProduct(mn) * Sn.Multiply(phi);

         return (mnp1, Snp1);
    }
    static public (Vector<double> mean, Matrix<double> cov) BatchWithSimplePrior(Matrix<double> Phi, Vector<double> y, double alpha, double beta)
    {
        int M = Phi.ColumnCount;
        Matrix<double> SNinv = alpha * Matrix<double>.Build.DenseIdentity(M) + beta * Phi.Transpose() * Phi;
        Vector<double> mN = SNinv.Solve(beta * Phi.Transpose() * y);
        Matrix<double> SN = SNinv.Inverse();
        return (mN, SN);
    }
    static public (double mean, double var) Predict(Vector<double> phi, Vector<double> mn, Matrix<double> Sn, double beta)
    {
        double mean = phi.DotProduct(mn);
        double var = 1.0 / beta + phi.DotProduct(Sn * phi);
        return (mean, var);
    }
    static public double ComputeLogEvidence(Matrix<double> Phi, Vector<double> y, Vector<double> mN, Matrix<double> SN, double alpha, double beta)
    {
        int M = Phi.ColumnCount;
        int N = Phi.RowCount;
        double EmN = beta / 2.0 * Math.Pow((y - Phi * mN).L2Norm(), 2) +
                     alpha / 2.0 * Math.Pow(mN.L2Norm(), 2);
	    double marginal_log_like = (M/2.0 * Math.Log(alpha) +
                                    N/2.0 * Math.Log(beta) -
                                    EmN +
                                    0.5 * Math.Log(SN.Determinant()) -
                                    N/2.0 * Math.Log(2 * Math.PI));
		return marginal_log_like;
    }
}
