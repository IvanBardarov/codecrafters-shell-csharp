using System.Text;

public static class UserInputHelpers
{
    public static List<string> ParseUserInput(string command, string input)
    {
        var ret = new List<string>();
        var strBldr = new System.Text.StringBuilder();

        if (string.IsNullOrWhiteSpace(input))
        {
            return ret;
        }

        var inputChars = input.ToCharArray();
        var isInsideSingleQuotes = false;
        var isInsideDoubleQuotes = false;

        for (var i = 0; i < inputChars.Length; i++)
        {
            var currentChar = inputChars[i];

            if (currentChar == '\\' && !isInsideDoubleQuotes && !isInsideSingleQuotes)
            {
                if (i + 1 < inputChars.Length)
                {
                    i++;
                    strBldr.Append(inputChars[i]);
                }
                else
                {
                    strBldr.Append(inputChars[i]);
                }
            }
            else if (currentChar == '\\' && isInsideDoubleQuotes)
            {
                if (i + 1 < inputChars.Length)
                {
                    i++;
                    if (inputChars[i] == '\\' || inputChars[i] == '"')
                    {
                        strBldr.Append(inputChars[i]);
                    }
                }
                else
                {
                    strBldr.Append(inputChars[i]);
                }
            }
            else if (currentChar == '\'' && !isInsideDoubleQuotes)
            {
                isInsideSingleQuotes = !isInsideSingleQuotes;
            }
            else if (currentChar == '"' && !isInsideSingleQuotes)
            {
                isInsideDoubleQuotes = !isInsideDoubleQuotes;
            }
            else if (char.IsWhiteSpace(currentChar) &&
                 !isInsideSingleQuotes &&
                 !isInsideDoubleQuotes)
            {
                if (strBldr.Length > 0)
                {
                    ret.Add(strBldr.ToString());
                    strBldr.Clear();
                }
            }
            else
            {
                strBldr.Append(currentChar);
            }
        }

        if (strBldr.Length > 0)
        {
            ret.Add(strBldr.ToString());
        }

        return ret;
    }

    public static string? ReadLineWithAutoComplete(string[] builtIns, string[]? folders)
    {
        var ret = new StringBuilder();

        while (true)
        {
            var consoleKeyInfo = Console.ReadKey(true);
            var originalInput = ret.ToString();

            if (consoleKeyInfo.KeyChar != '\0' && !char.IsControl(consoleKeyInfo.KeyChar))
            {
                ret.Append(consoleKeyInfo.KeyChar);
                Console.Write(consoleKeyInfo.KeyChar);
                continue;
            }

            if (consoleKeyInfo.Key == ConsoleKey.Backspace && ret.Length > 0)
            {
                ret.Length--;
                Console.Write("\b \b");
                continue;
            }

            if (consoleKeyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return ret.ToString();
            }

            if (consoleKeyInfo.Key == ConsoleKey.Tab && !string.IsNullOrWhiteSpace(originalInput))
            {
                var prefix = originalInput.TrimEnd();
                var builtInMatch = builtIns
                    .FirstOrDefault(o => o.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                string? autoCompletedInput = null;

                if (!string.IsNullOrWhiteSpace(builtInMatch))
                    autoCompletedInput = builtInMatch;
                else
                {
                    if (folders != null)
                    {
                        try
                        {
                            foreach (var folder in folders)
                            {
                                if (Directory.Exists(folder))
                                {
                                    var files = Directory.GetFiles(folder);
                                    var fileNames = files.Select(o => Path.GetFileNameWithoutExtension(o)).ToArray();
                                    var executableExternalMatch = fileNames
                                        .FirstOrDefault(o => o.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                                    if (!string.IsNullOrWhiteSpace(executableExternalMatch))
                                    {
                                        autoCompletedInput = executableExternalMatch;
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }


                if (!string.IsNullOrWhiteSpace(autoCompletedInput))
                {
                    Console.Write($"\r$ {new string(' ', originalInput.Length)}");
                    Console.Write($"\r$ ");

                    ret.Clear();
                    ret.Append(autoCompletedInput + " ");
                    Console.Write(ret.ToString());
                }
                else
                {
                    if (OperatingSystem.IsWindows())
                        Console.Beep();
                    else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                        Console.Write("\x07");
                }
            }
        }
    }
}