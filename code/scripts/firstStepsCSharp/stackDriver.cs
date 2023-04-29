class Example
{
    public static void Main()
    {
        var s = new Acme.Collections.Stack<int>();
        s.Push(1); // stack contains 1
        s.Push(10); // stack contains 1, 10
        s.Push(100); // stack contains 1, 10, 100
        System.Console.WriteLine(s.Pop()); // stack contains 1, 10
        System.Console.WriteLine(s.Pop()); // stack contains 1
        System.Console.WriteLine(s.Pop()); // stack is empty
    }
}
