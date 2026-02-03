class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");
            
            string? userInput = Console.ReadLine();

            string[]? userInputArray = userInput?.Split(' ');

            var coreEngine = new CoreEngine(userInputArray);

            if(coreEngine.Result == null)
            {
                break;
            }
            else
            {
                Console.WriteLine(coreEngine.Result);
            }           
        }
    }
}
