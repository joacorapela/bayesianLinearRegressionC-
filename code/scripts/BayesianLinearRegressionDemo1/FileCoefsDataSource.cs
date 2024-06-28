using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;

public class FileCoefsDataSource
{
    public IObservable<Vector<double>> Process(IObservable<FileSystemEventArgs> source)
    {
        Console.WriteLine("FileCoefsDataSource::Process called");
        System.Random rng = SystemRandomSource.Default;

        return source.Select( input =>
        {
            Console.WriteLine("FileCoefsDataSource::Process generating new regression coefficients");
            Console.WriteLine($"Received input {input}");
            Console.WriteLine($"input.ChangeType {input.ChangeType}");
            Console.WriteLine($"input.FullPath {input.FullPath}");
            Vector<double> coefs = this._readCoefs(coefsFilename: input.FullPath);
            Console.WriteLine("New coefficients: " + coefs);
            return coefs;
        });
    }

    private Vector<double> _readCoefs(string coefsFilename)
    {
        List<double> values = new List<double>();
        using (var rd = new StreamReader(coefsFilename))
        {
            if (!rd.EndOfStream)
            {
                var splits = rd.ReadLine().Split(',');
                foreach (string split in splits)
                {
                    var splitFloat = Convert.ToDouble(split);
                    values.Add(splitFloat);
                }
            }
        }
        Vector<double> answer = Vector<double>.Build.DenseOfEnumerable(values);

        return answer;
    }
}
