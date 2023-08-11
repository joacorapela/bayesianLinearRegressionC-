using System;
using MathNet.Numerics.LinearAlgebra;

namespace JoacoRapela.Statistics
{
    public class PosteriorDataItem
    {
        public Vector<double> mn;
        public Matrix<double> Sn;
    }
}
