class Program
{
    static void Main()
    {
        Console.Write("$ ");

        // Captures the user's command in the "command" variable
        string command = Console.ReadLine();

        Console.WriteLine($"{command}: command not found");
    }
}
