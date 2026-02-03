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

            if(command.Result == null)
            {
                break;
            }
            else
            {
                Console.WriteLine(command.Result);
            }           
        }
    }
}
