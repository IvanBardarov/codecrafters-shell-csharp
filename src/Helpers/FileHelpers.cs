public static class FileHelpers
{
    public static void RedirectToFile(string? stdOutTxt, string filePath, TypeOfOperator typeOfOperator)
    {
        try
        {
            if (!IsValidFilePath(filePath))
                throw new Exception($"Path \"{filePath}\" is not valid!");

            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);

            var fileName = Path.GetFileName(fullPath);
            if (string.IsNullOrWhiteSpace(fileName))
                throw new Exception($"There is no file specified in the path  {fullPath}!");

            var root = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
            if (Directory.Exists(directory))
            {
                root = Directory.GetDirectoryRoot(fullPath);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(directory))
                    throw new Exception($"There is no any folder specified in the path {fullPath}!");

                Directory.CreateDirectory(Path.Combine(root, directory));
            }

            if (!string.IsNullOrWhiteSpace(directory))
            {
                fullPath = Path.Combine(root, directory, fileName);
                stdOutTxt = string.IsNullOrWhiteSpace(stdOutTxt) ? "" : stdOutTxt + Environment.NewLine;

                if (typeOfOperator == TypeOfOperator.AppendStdOut
                    || typeOfOperator == TypeOfOperator.AppendStdErr)
                {
                    File.AppendAllText(fullPath, stdOutTxt);
                }
                else
                {
                    File.WriteAllText(fullPath, stdOutTxt);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static bool IsValidFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var filePathArray = filePath
            .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        var ret = true;
        foreach (var el in filePathArray.Skip(1))
        {
            if (el.Contains(':'))
            {
                ret = false;
                break;
            }
        }

        return ret;
    }

    public static bool IsFileExecutable(string[]? files, string? fileName, ref string? executableFolder,
    ref List<string> executableExternalCommands)
    {
        //var executableExternals = new List<string>();
        var ret = false;
        var searchIn = files?.Where(o => Path.GetFileNameWithoutExtension(o) == fileName).ToList();
        if (searchIn != null)
        {
            foreach (var e in searchIn)
            {
                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
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
                    executableExternalCommands.Add(e);
                    executableExternalCommands.Sort();
                    executableExternalCommands.Distinct();
                    executableFolder = Path.GetFullPath(e);
                    break;
                }
            }
        }

        return ret;
    }

    public static List<string> GetExecutableCommands(string[]? folders)
    {
        var ret = new List<string>();

        if(folders != null)
        {
            foreach(var folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    var files = Directory.GetFiles(folder);
                    foreach(var file in files)
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                        if(!string.IsNullOrWhiteSpace(fileNameWithoutExtension) && !ret.Contains(fileNameWithoutExtension))
                        {
                            ret.Add(fileNameWithoutExtension);
                        }
                    }
                }
            }
        }

        ret.Sort();
        return ret;
    }
}
