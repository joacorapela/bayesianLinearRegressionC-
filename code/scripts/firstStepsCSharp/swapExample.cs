
class Example
{
    static void Swap(ref int x, ref int y)
    {
        int temp = x;
        x = y;
        y = temp;
    }

    public static void Main()
    {
            int i = 1, j = 2;
            Swap(ref i, ref j);
            System.Console.WriteLine($"{i} {j}");    // "2 1"
    }
}
