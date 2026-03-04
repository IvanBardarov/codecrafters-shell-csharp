class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var initialCommandObject = new Commands();
            var builtIns = initialCommandObject.BuiltInsArray;
            var folders = initialCommandObject.Folders;

            string? consoleInput = UserInputHelpers.ReadLineWithAutoComplete(builtIns, folders);
            string? userInput = string.Empty;

            if (!string.IsNullOrWhiteSpace(consoleInput))
            {
                userInput = consoleInput;
                consoleInput = null;
            }

            var command = new Commands(userInput);

            bool flowControl = FlowControlHelpers.FlowControl(command);
            if (!flowControl)
            {
                break;
            }
        }
    }
}