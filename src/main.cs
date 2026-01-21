class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            // Captures the user's command in the "command" variable
            string? command = Console.ReadLine();

            if(command == "exit")
            {
                break;
            }

            Console.WriteLine($"{command}: command not found");
        }

    }
}
