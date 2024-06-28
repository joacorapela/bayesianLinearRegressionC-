using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public class RegressionUtils
{
    public static List<Func<double, double>> GetPolynomialBasisFunctions(int M)
    {
        var basisFunctions = new List<Func<double, double>>(M + 1);

        for (int m = 0; m <= M; m++)
        {
            int exponent = m;  // Capture the current value of m
            basisFunctions.Add(x => Math.Pow(x, exponent));
        }

        return basisFunctions;
    }
    public static Vector<double> BuildDesignMatrixRow(double x, List<Func<double, double>>  basisFunctions)
	{
		int nElem = basisFunctions.Count;
		Vector<double> designMatrixRow = Vector<double>.Build.Dense(nElem);
		for (int i = 0; i <nElem; i++)
		{
			designMatrixRow[i] = basisFunctions[i](x);
		}
		return designMatrixRow;
	}
    public static Matrix<double> BuildDesignMatrix(Vector<double> x, List<Func<double, double>>  basisFunctions)
	{
		int nCol = basisFunctions.Count;
		int N = x.Count;
		Matrix<double> designMatrix = Matrix<double>.Build.Dense(N, nCol);
		for (int n = 0; n < N; n++)
		{
			designMatrix.SetRow(n, BuildDesignMatrixRow(x[n], basisFunctions));
		}
		return designMatrix;
	}
}
