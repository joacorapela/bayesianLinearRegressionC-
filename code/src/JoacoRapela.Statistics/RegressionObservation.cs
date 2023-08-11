using System;

namespace JoacoRapela.Statistics
{
    public struct RegressionObservation
    {
        public RegressionObservation(double x, double t)
        {
            this.x = x;
            this.t = t;
        }

        public double x;
        public double t;
    }
}