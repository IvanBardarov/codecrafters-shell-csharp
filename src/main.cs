class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");
            
            string? consoleInput = Console.ReadLine();
            string userInput = string.Empty;

            if (!string.IsNullOrWhiteSpace(consoleInput))
            {
                userInput = consoleInput;
            }

            var command = new Commands(userInput);

            if(command.Result == "exit")
            {
                break;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(command.RedirectToFile))
                {                    
                    if (command.Error && string.IsNullOrWhiteSpace(command.Result))
                    {
                        Console.WriteLine(command.ErrorMessage);
                    }
                    else if(command.Error && !string.IsNullOrWhiteSpace(command.Result))
                    {
                        Console.WriteLine(command.ErrorMessage);
                        Helpers.RedirectToFile(command.Result ?? "", command.RedirectToFile);
                    }
                    else
                    {
                        Helpers.RedirectToFile(command.Result ?? "", command.RedirectToFile);
                    }
                }
                else if(!string.IsNullOrWhiteSpace(command.Result))
                {
                    Console.WriteLine(string.Join(" ", command.Result));
                } 
                else if(!string.IsNullOrWhiteSpace(command.ErrorMessage))
                {
                    Console.WriteLine(string.Join(" ", command.ErrorMessage));
                }
            }           
        }
    }
}