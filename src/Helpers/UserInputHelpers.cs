using System.Collections.Immutable;
using System.Diagnostics.Metrics;
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
        var tabCounter = 0;

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
                tabCounter++;

                var prefix = originalInput.TrimEnd();
                string? autoCompletedInput = null;

                var matches = new List<string>();
                var builtInMatch = builtIns
                    .FirstOrDefault(o => o.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

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

                                    matches.AddRange(fileNames
                                        .Where(o => o.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                        .ToList());

                                    matches.Sort();
                                    matches = matches.Distinct().ToList();
                                }
                            }
                            autoCompletedInput = matches.Count == 1 ? matches[0] : prefix;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                if(!string.IsNullOrWhiteSpace(autoCompletedInput) &&
                    (!string.IsNullOrWhiteSpace(builtInMatch) || matches.Count > 0))
                    RefreshTheInputAfterTabKeyPressed(ret, originalInput, autoCompletedInput, matches, tabCounter);
                else
                    BeepAfterTabKeyPressedForTheFirstTime();
            }
        }
    }

    private static void RefreshTheInputAfterTabKeyPressed(StringBuilder ret, string originalInput, string autoCompletedInput,
        List<string> matches, int tabCounter)
    {
        var matchesOutput = matches.Count > 0 ? $"{string.Join("  ", matches).Trim()}" : string.Empty;
        ret.Clear();

        var output = string.Empty;

        switch (matches.Count)
        {
            case 0:
                output = autoCompletedInput.Length >= originalInput.Length ? autoCompletedInput + " " : originalInput;
                ret.Append(autoCompletedInput + " ");
                break;
            case 1:
                output = autoCompletedInput + " ";
                ret.Append(autoCompletedInput + " ");
                break;
            case >= 2:
                output = autoCompletedInput;
                ret.Append(autoCompletedInput);
                if(tabCounter == 1)
                    BeepAfterTabKeyPressedForTheFirstTime();
                break;
            default:
                break;
        }

        if (matches.Count > 1 && tabCounter >= 2)
        {
            Console.WriteLine();
            Console.WriteLine(matchesOutput);
            Console.Write($"\r$ ");
        }
        else
        {
            Console.Write($"\r$ {new string(' ', originalInput.Length)}");
            Console.Write($"\r$ ");
        }

        Console.Write(output);
    }

    private static void BeepAfterTabKeyPressedForTheFirstTime()
    {
        if (OperatingSystem.IsWindows())
            Console.Beep();
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            Console.Write("\x07");
    }
}