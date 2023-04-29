using System;

class Hello
{
    static void Main(string[] args)
    {
        bool flag = false;
        int value = 0;
        if (flag)
        {
            value = 10;
            Console.WriteLine("Inside of code block: " + value);
        }
        Console.WriteLine($"Outisde of code block: {value}");
    }
}
