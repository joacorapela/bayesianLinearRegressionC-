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
}
