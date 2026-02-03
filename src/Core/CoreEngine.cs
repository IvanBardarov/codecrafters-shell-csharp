public class CoreEngine
{
    public string? Command { get; private set; }
    public string[]? Arguments { get; private set; }
    public string? Result { get; private set;}
    public string? PathEnvironment { get; private set; }
    public string[]? Folders { get; private set; }  
    public bool IsExecutable { get; private set; }  

    public delegate string? SetResult(string? command, string[]? args);

    public CoreEngine(){}

    public CoreEngine(string[]? userInput)
    {
        string? command = userInput?[0];
        string[]? args = userInput?[1..];
        BuiltIns builtIns = new BuiltIns();
        PathEnvironment = Environment.GetEnvironmentVariable("PATH");
        Folders = PathEnvironment?.Split(Path.PathSeparator);

        if (builtIns.BuiltInsArray.Contains(command))
        {
            Command = command;
            Arguments = args;

            switch (Command)
            {
                case "exit":
                    Result = ReturnResult(Exit);
                    break;
                case "echo":
                    Result = ReturnResult(Echo);
                    break;
                case "type":
                    Result = ReturnResult(Type);
                    break;
                default:
                    Result = null;
                    break;
            }
        }
        else
        {
            Result = $"{command}: command not found";
        }
    }

    private string? Exit(string? command, string[]? args)
    {
        Result = null;
        return Result;
    }

    private string? Echo(string? command, string[]? args)
    {
        if(args != null)
        {
            Result = string.Join(" ", args);            
        }
        
        return Result;
    }

    private string? Type(string? command, string[]? args)
    {
        if(args != null)
        {
            string arguments = string.Join(" ", args);
            if(arguments == "echo" || arguments == "exit" || arguments == "type")
            {
                Result = $"{arguments} is a shell builtin";
            }
            else
            {
                var count = 0;
                if(Folders != null)
                {
                    foreach (var folder in Folders)
                    {
                        count++;
                        if (Directory.Exists(folder))
                        {
                            var files = Directory.GetFiles(folder);
                            var fileNames = files.Select(o => Path.GetFileName(o)).ToArray();

                            if (IsFileExecutable(files, arguments))
                            {
                                IsExecutable = true;
                                Result = $"{arguments} is {Path.Combine(folder, arguments)}";
                                break;
                            }
                        }
                    }
                    if (!IsExecutable)
                    {
                        Result = $"{arguments}: not found";
                    }
                }
                else
                {
                    Result = $"{arguments}: not found";
                }                
            }
        }

        return Result;
    }

    private bool IsFileExecutable(string[]? files, string fileName)
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

                    if (ret)
                    {
                        break;
                    }
                }
                else
                {
                    var extension = Path.GetExtension(e).ToLowerInvariant();
                    ret = extension is ".exe" or ".bat" or ".cmd" or "ps1" or ".dll";
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