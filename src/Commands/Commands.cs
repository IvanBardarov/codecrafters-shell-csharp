using System.Diagnostics;

public sealed class Commands
{
    public readonly string[] BuiltInsArray =
    {
        "echo",
        "exit",
        "type",
        "pwd",
        "cd"
    };

    public string? Command { get; private set; }
    public List<string> Arguments { get; private set; } = new List<string>();
    public string? Result { get; private set; }
    public string? PathEnvironment { get; private set; }
    public string[]? Folders { get; private set; }
    public string[]? Files { get; private set; }
    public bool IsExecutable { get; private set; }
    public string? ExecutableFolder { get; private set; }
    public List<string> ExecutableExternalCommands { get; private set; } = new List<string>();
    public string? RedirectToFile { get; private set; }
    public TypeOfOperator TypeOfOperator { get; private set; }
    public bool Error { get; private set; }
    public string? ErrorMessage { get; private set; }

    public delegate string SetResult(List<string> args);

    public Commands()
    {
        PathEnvironment = Environment.GetEnvironmentVariable("PATH");
        Folders = PathEnvironment?.Split(Path.PathSeparator);

        ExecutableExternalCommands = FileHelpers.GetExecutableCommands(Folders);
    }

    public Commands(string? userInput) : this()
    {
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            var tokens = UserInputHelpers.ParseUserInput(string.Empty, userInput);

            if (tokens.Count > 0)
            {
                Command = tokens[0];
                var operatorIndex = tokens.FindIndex(o => o == ">"
                    || o == "1>" || o == "2>" || o == ">>" || o == "1>>" || o == "2>>");
                if (operatorIndex > 1)
                {
                    Arguments = tokens.Skip(1)
                        .Take(operatorIndex - 1)
                        .ToList();

                    switch (tokens[operatorIndex])
                    {
                        case ">":
                        case "1>":
                            TypeOfOperator = TypeOfOperator.RedirectStdOut;
                            break;
                        case "2>":
                            TypeOfOperator = TypeOfOperator.RedirectStdErr;
                            break;
                        case ">>":
                        case "1>>":
                            TypeOfOperator = TypeOfOperator.AppendStdOut;
                            break;
                        case "2>>":
                            TypeOfOperator = TypeOfOperator.AppendStdErr;
                            break;
                        default:
                            TypeOfOperator = TypeOfOperator.None;
                            break;
                    }

                    RedirectToFile = string.Join(" ", tokens
                        .Skip(operatorIndex + 1)
                        .ToList());
                }
                else
                {
                    Arguments = tokens.Skip(1).ToList();
                    RedirectToFile = null;
                }
            }
            else
            {
                Command = null;
                Arguments = new List<string>();
                RedirectToFile = null;
            }

            if (BuiltInsArray.Contains(Command))
            {
                switch (Command)
                {
                    case "exit":
                        Result = ReturnResult(ExitBuiltIn);
                        break;
                    case "echo":
                        Result = ReturnResult(EchoBuiltIn);
                        break;
                    case "type":
                        Result = ReturnResult(TypeBuiltIn);
                        break;
                    case "pwd":
                        Result = ReturnResult(PwdBuiltIn);
                        break;
                    case "cd":
                        CdBuiltIn(Arguments);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (Folders != null)
                {
                    foreach (var folder in Folders)
                    {
                        if (Directory.Exists(folder))
                        {
                            Files = Directory.GetFiles(folder);

                            ExecutableExternalCommands.AddRange(Files);

                            string? executableFolder = null;
                            var executableExternalCommands = new List<string>();

                            if (FileHelpers.IsFileExecutable(Files, Command, ref executableFolder, ref executableExternalCommands))
                            {
                                ExecutableFolder = executableFolder;
                                ExecutableExternalCommands = executableExternalCommands;
                                using Process? process = ExternalCommand();

                                break;
                            }
                        }
                    }
                    if (Result == null)
                    {
                        Result = $"{Command}: command not found";
                    }
                }
                else
                {
                    Result = $"{Command}: command not found";
                }
            }
        }
    }

    private Process? ExternalCommand()
    {
        var executableDir = Path.GetDirectoryName(ExecutableFolder);

        var startInfo = new ProcessStartInfo
        {
            FileName = Command,
            UseShellExecute = false,
            WorkingDirectory = executableDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var arg in Arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }
        var process = Process.Start(startInfo);

        if (process != null)
        {
            process?.WaitForExit();

            var stdout = process?.StandardOutput.ReadToEnd();
            var stderr = process?.StandardError.ReadToEnd();

            Result = stdout?.TrimEnd();
            ErrorMessage = stderr?.TrimEnd();

            if (!string.IsNullOrWhiteSpace(stderr))
                Error = true;
        }

        return process;
    }

    public string ExitBuiltIn(List<string> args)
    {
        var ret = "exit";
        return ret;
    }

    private string EchoBuiltIn(List<string> args)
    {
        string ret = string.Empty;

        if (args != null && args.Count > 0)
        {
            ret = string.Join(" ", args);
        }

        return ret;
    }

    public string TypeBuiltIn(List<string> args)
    {
        string ret = string.Empty;

        if (args != null && args.Count > 0)
        {
            string arguments = string.Join(" ", args);
            if (BuiltInsArray.Contains(arguments))
            {
                ret = $"{arguments} is a shell builtin";
            }
            else
            {
                if (Folders != null)
                {
                    foreach (var folder in Folders)
                    {
                        if (Directory.Exists(folder))
                        {
                            var files = Directory.GetFiles(folder);
                            var fileNames = files.Select(o => Path.GetFileName(o)).ToArray();

                            string? executableFolder = null;
                            var executableExternalCommands = new List<string>();

                            if (FileHelpers.IsFileExecutable(files, arguments, ref executableFolder, ref executableExternalCommands))
                            {
                                ExecutableFolder = executableFolder;
                                ExecutableExternalCommands = executableExternalCommands;
                                IsExecutable = true;
                                ret = $"{arguments} is {Path.Combine(folder, arguments)}";
                                break;
                            }
                        }
                    }
                    if (!IsExecutable)
                    {
                        ret = $"{arguments}: not found";
                    }
                }
                else
                {
                    ret = $"{arguments}: not found";
                }
            }
        }

        return ret;
    }

    public string PwdBuiltIn(List<string> args)
    {
        var ret = Directory.GetCurrentDirectory();
        return ret;
    }

    public void CdBuiltIn(List<string> args)
    {
        string path = string.Join(" ", args);

        if (path == "~")
        {
            path = Environment.GetEnvironmentVariable("HOME")
                ?? Environment.GetEnvironmentVariable("USERPROFILE")
                ?? throw new InvalidOperationException("No HOME or USERPROFILE environment variable found.");

            Directory.SetCurrentDirectory(path);
        }
        else if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
        {
            Directory.SetCurrentDirectory(path);
        }
        else
        {
            Console.WriteLine($"cd: {path}: No such file or directory");
        }
    }

    private string ReturnResult(SetResult result)
    {
        SetResult _result = result;
        return _result(Arguments);
    }

}