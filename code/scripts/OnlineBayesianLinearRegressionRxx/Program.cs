
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra;

// See https://aka.ms/new-console-template for more information
int n_samples = 20;
double prior_precision_coef = 2.0;
string data_filename_pattern = "../../../data/linearRegression_nSamples{0:D2}.xml";

string data_filename = String.Format(data_filename_pattern, n_samples);
FileStream fs = new FileStream(data_filename, FileMode.Open);
XmlSerializer serializer = new XmlSerializer(typeof(Result));
Result result = (Result) serializer.Deserialize(fs);

double[] x = result.x;
double[] t = result.t;
double sigma = result.sigma;
double likelihood_precision_coef = Math.Pow((1.0/sigma), 2);

double[] aux = {0.0, -0.0};
Vector<double> m0 = Vector<double>.Build.DenseOfArray(aux);
Matrix<double> S0 = 1.0 / prior_precision_coef * Matrix<double>.Build.DenseIdentity(2);

RegressionObservationsDataSource dataSource = new RegressionObservationsDataSource(x=x, t=t);
OnlineBayesianLinearRegression oblr = new OnlineBayesianLinearRegression(prior_precision_coef, likelihood_precision_coef, m0, S0);

dataSource.Subscribe(oblr);

bool exit = false;
int i = 0;
while (!exit)
{
    Console.WriteLine(oblr.mn.ToString());
    Console.WriteLine(oblr.Sn.ToString());

    // i++;
    // Console.WriteLine(String.Format("Processed {0:D2} samples", i));
    Console.WriteLine("Presss ENTER to continue and other key to exit");
    String cont = Console.ReadLine();
    if (!cont.Equals("") | dataSource.done)
    {
        break;
    }

    dataSource.PublishNextObservation();
}
