class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            // Captures the user's command in the "command" variable
            string? userInput = Console.ReadLine();

            string[]? userInputArray = userInput?.Split(' ');

            string? command = userInputArray?[0];
            string[]? args = userInputArray?[1..];
            if(command == "exit")
            {
                break;
            }
            else if(command == "echo")
            {
                if(args != null)
                {
                    string output = string.Join(" ", args);
                    Console.WriteLine(output);
                }
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }            
        }

    }
}
