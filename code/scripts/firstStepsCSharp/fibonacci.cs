using System;
using System.Collections.Generic;

class MainClass
{
        static void Main(string[] args)
        {
                var fibonacciNumbers = new List<int> {1, 1};
                int numel = 20;

                int counter = 3;
                while (counter <= numel)
                {
                    int previous = fibonacciNumbers[fibonacciNumbers.Count - 1];
                    int previous2 = fibonacciNumbers[fibonacciNumbers.Count - 2];
                    fibonacciNumbers.Add( previous + previous2 );
                    counter = counter + 1;
                }

                foreach(int number in fibonacciNumbers)
                {
                        Console.WriteLine(number);
                }
        }
}
