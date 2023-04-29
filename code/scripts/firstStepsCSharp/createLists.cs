using System;
using System.Collections.Generic;

class MainClass
{
    static void Main(string[] args)
    {
        var names = new List<string> { "<name>", "Ana", "Felipe" };
        
        foreach (var name in names)
        {
                Console.WriteLine($"Hello {name.ToUpper()}!");
        }

        names.Add("Maria");
        names.Add("Bill");
        names.Remove("Ana");
       
        Console.WriteLine(); 
        foreach (var name in names)
        {
                Console.WriteLine($"Hello {name.ToUpper()}!");
        }
    }
}
