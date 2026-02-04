class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");
            
            string? userInput = Console.ReadLine();

            string[]? userInputArray = userInput?.Split(' ');

            var command = new Commands(userInputArray);

            if(command.Result == "exit")
            {
                break;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(command.Result))
                {
                    Console.WriteLine(command.Result);
                }            
            }           
        }
    }
}
