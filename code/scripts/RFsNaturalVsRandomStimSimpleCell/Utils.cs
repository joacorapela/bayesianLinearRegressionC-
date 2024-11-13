
using System;
using System.IO;

public class CSVReader
{
    public static double[] ReadCSVToVector(string filename, string sep=",")
    {
        // Read all lines from the CSV file
        string[] lines = File.ReadAllLines(filename);
        string[] values_str = lines[0].Split(sep);

        // Determine the dimensions of the matrix
        int numElem = values_str.Length;

        // Create the double vector
        double[] values_double = new double[numElem];

        // Fill the matrix with the data from the CSV
        for (int i = 0; i < numElem; i++)
        {
            // Attempt to parse each value to double
            if (double.TryParse(values_str[i], out double result))
            {
                values_double[i] = result;
            }
            else
            {
                throw new FormatException($"Unable to parse '{values_str[i]}' as a double at element {i}.");
            }
        }

        return values_double;
    }

    public static double[,] ReadCSVToMatrix(string filename, string sep=",")
    {
        string[] lines = File.ReadAllLines(filename);
        int nRows = lines.Length;
        string[] values_str = lines[0].Split(sep);
        int nCols = values_str.Length;
        Console.WriteLine($"nRows={nRows}, nCols={nCols}");
        double[,] answer = new double[nRows, nCols];
        for (int i = 0; i < nRows; i++)
        {
            values_str = lines[i].Split(sep);
            for (int j = 0; j < nCols; j++)
            {
                if(double.TryParse(values_str[j], out double result))
                {
                    answer[i, j] = result;
                }
                else
                {
                    throw new FormatException($"Unable to parse '{values_str[j]}' as a double at row {i} and col {j}.");
                }
            }
        }
        return answer;
    }
}

