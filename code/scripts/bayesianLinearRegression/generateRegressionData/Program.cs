// See https://aka.ms/new-console-template for more information
using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using MathNet.Numerics.Data.Matlab;

// Define data generation variables

int n_samples = 20;
double a0    = -0.3;
double a1    = 0.5;
double sigma = 0.2;
string data_filename_pattern = "../data/linearRegression_nSamples{0:D2}.mat";
string fig_filename_pattern = "../figures/regression_data_nSamples{0:D2}.{1}";

// Sample data
Vector<double> x = CreateVector.Random<double>(n_samples, new ContinuousUniform(-1, 1));

Vector<double> y = a0 + a1 * x;
Vector<double> epsilon = CreateVector.Random<double>(y.Count, new Normal(0, sigma));

Vector<double> t = y + epsilon;

[Serializable]
class Result
{
    Vector<double> x;
    Vector<double> y;
    Vector<double> t;
    double a0;
    double a1;
    double sigma;
}

Result result = new Result();
result.x = x;
result.y = y;
result.t = t;
result.a0 = a0;
result.a1 = a1;
result.sigma = sigma;

string data_filename = String.Format(data_filename_pattern, n_samples);
File f = new File(data_filename);
Stream s = f.Open(FileMode.Create);
BinaryFormatter b = new BinaryFormatter();
b.Serialize(s, result);
s.Close();

Console.WriteLine("Done");
