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
    public string? RedirectToFile { get; private set; }
    public bool Error { get; private set; }
    public string? ErrorMessage { get; private set; }

    public delegate string SetResult(List<string> args);

    public Commands(){ }

    public Commands(string userInput)
    {
        if(!string.IsNullOrWhiteSpace(userInput))
        {
            PathEnvironment = Environment.GetEnvironmentVariable("PATH");
            Folders = PathEnvironment?.Split(Path.PathSeparator);

            var tokens = Helpers.ParseUserInput(string.Empty, userInput);

            if (tokens.Count > 0)
            {
                Command = tokens[0];
                var redirectToFileIndex = tokens.FindIndex(o => o == ">" || o == "1>");
                if(redirectToFileIndex > 1)
                {
                    Arguments = tokens.Skip(1)
                        .Take(redirectToFileIndex - 1)
                        .ToList();
                    RedirectToFile = string.Join(" ", tokens
                        .Skip(redirectToFileIndex + 1)
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
                if(Folders != null)
                {
                    foreach (var folder in Folders)
                    {
                        if (Directory.Exists(folder))
                        {
                            Files = Directory.GetFiles(folder);

                            if (IsFileExecutable(Files, Command))
                            {
                                using Process? process = ExternalCommand();

                                break;
                            }
                        }
                    }
                    if(Result == null)
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

        if(args != null && args.Count > 0)
        {
            ret = string.Join(" ", args);    
        }
        
        return ret;
    }

    public string TypeBuiltIn(List<string> args)
    {
        string ret = string.Empty;

        if(args != null && args.Count > 0)
        {
            string arguments = string.Join(" ", args);
            if(BuiltInsArray.Contains(arguments))
            {
                ret = $"{arguments} is a shell builtin";
            }
            else
            {
                if(Folders != null)
                {
                    foreach (var folder in Folders)
                    {
                        if (Directory.Exists(folder))
                        {
                            var files = Directory.GetFiles(folder);
                            var fileNames = files.Select(o => Path.GetFileName(o)).ToArray();

                            if (IsFileExecutable(files, arguments))
                            {
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

        if(path == "~")
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

    private bool IsFileExecutable(string[]? files, string? fileName)
    {
        var ret = false; 
        var searchIn = files?.Where(o => Path.GetFileNameWithoutExtension(o) == fileName).ToList();
        if(searchIn != null)
        {
            foreach(var e in searchIn)
            {
                if(OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    var fileInfo = new FileInfo(e);
                    var mode = fileInfo.UnixFileMode;

                    ret = (mode & UnixFileMode.UserExecute) != 0
                        || (mode & UnixFileMode.GroupExecute) != 0
                        || (mode & UnixFileMode.OtherExecute) != 0;
                }
                else
                {
                    var extension = Path.GetExtension(e).ToLowerInvariant();
                    ret = extension is ".exe" or ".bat" or ".cmd" or ".ps1" or ".dll";
                }
                if (ret)
                {
                    ExecutableFolder = Path.GetFullPath(e);
                    break;
                }
            }
        }

        return ret;
    }

    private string ReturnResult(SetResult result)
    {
        SetResult _result = result;
        return _result(Arguments);
    }

}