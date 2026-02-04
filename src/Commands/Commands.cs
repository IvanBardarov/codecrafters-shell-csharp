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
    public string[]? Arguments { get; private set; }
    public string? Result { get; private set; }
    public string? PathEnvironment { get; private set; }
    public string[]? Folders { get; private set; }  
    public string[]? Files { get; private set; }
    public bool IsExecutable { get; private set; }  
    public string? ExecutableFolder { get; private set; }

    public delegate string? SetResult(string? command, string[]? args);

    public Commands()
    {
        
    }

    public Commands(string[]? userInput)
    {
        string? command = userInput?[0];
        string[]? args = userInput?[1..];
        
        PathEnvironment = Environment.GetEnvironmentVariable("PATH");
        Folders = PathEnvironment?.Split(Path.PathSeparator);

        Command = command;
        Arguments = args;

        if (BuiltInsArray.Contains(command))
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
                    CdBuiltIn(Command, Arguments);
                    break;
                default:
                    Result = null;
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
                            var argsToPass = Arguments != null && Arguments.Length > 0 ?
                            string.Join(" ", Arguments) : "";
                            
                            var executableDir = Path.GetDirectoryName(ExecutableFolder);                                                  
			  
                            using var process = Process.Start(new ProcessStartInfo {
                                FileName = Command,
                                Arguments = argsToPass,
                                WorkingDirectory = executableDir,
                                UseShellExecute = false, 
                                RedirectStandardOutput = true 
                            }); 

                            process?.WaitForExit();
                            Result = process?.StandardOutput.ReadToEnd().TrimEnd();
                            break;
                        }
                    }
                }
                if(Result == null)
                {
                    Result = $"{command}: command not found";
                }
            }
            else
            {
                Result = $"{command}: command not found";
            }            
        }
    }

    public string? ExitBuiltIn(string? command, string[]? args)
    {
        string? ret = "exit";
        return ret;
    }

    private string? EchoBuiltIn(string? command, string[]? args)
    {
        string? ret = string.Empty;

        if(args != null)
        {
            ret = string.Join(" ", args);            
        }
        
        return ret;
    }

    public string? TypeBuiltIn(string? command, string[]? args)
    {
        string? ret = string.Empty;

        if(args != null)
        {
            string arguments = string.Join(" ", args);
            if(BuiltInsArray.Contains(arguments)/*arguments == "echo" || arguments == "exit" || arguments == "type"*/)
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

    public string? PwdBuiltIn(string? command, string[]? args)
    {        
        string? ret = Directory.GetCurrentDirectory();
        return ret;
    }

    public void CdBuiltIn(string? command, string[]? args)
    {     
        string? path = args == null ? null : string.Join(" ", args);

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
                    ret = extension is ".exe" or ".bat" or ".cmd" or "ps1" or ".dll";
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

    private string? ReturnResult(SetResult result)
    {
        SetResult _result = result;
        return _result(Command, Arguments);
    }

}