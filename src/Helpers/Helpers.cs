using Microsoft.Win32.SafeHandles;

public static class Helpers
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

            if(currentChar == '\\' && !isInsideDoubleQuotes && !isInsideSingleQuotes)
            {  
                if(i + 1 < inputChars.Length)
                {
                    i++;
                    strBldr.Append(inputChars[i]);
                }
                else
                {
                    strBldr.Append(inputChars[i]);
                }      
            }
            else if(currentChar =='\\' && isInsideDoubleQuotes)
            {
                if(i + 1 < inputChars.Length)
                {
                    i++;
                    if(inputChars[i] == '\\' || inputChars[i]  == '"')
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
            else if(currentChar == '"' && !isInsideSingleQuotes)
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

    public static void RedirectToFile(string stdOutTxt, string filePath)
    {
        try
        {
            if(!IsValidFilePath(filePath))
                throw new Exception($"Path \"{filePath}\" is not valid!");

            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);

            var fileName = Path.GetFileName(fullPath);
            if(string.IsNullOrWhiteSpace(fileName))
                throw new Exception($"There is no file specified in the path  {fullPath}!");

            var root = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
            if (Directory.Exists(directory))
            {
                root = Directory.GetDirectoryRoot(fullPath);
            }
            else
            {
                if(string.IsNullOrWhiteSpace(directory))
                    throw new Exception($"There is no any folder specified in the path {fullPath}!");

                Directory.CreateDirectory(Path.Combine(root, directory));
            }

            if (!string.IsNullOrWhiteSpace(directory))
            {
                fullPath = Path.Combine(root, directory, fileName);
                File.WriteAllText(fullPath, stdOutTxt);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static bool IsValidFilePath(string filePath)
    {
        if(string.IsNullOrWhiteSpace(filePath))
            return false;

        var filePathArray = filePath
            .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        var ret = true;
        foreach(var el in filePathArray.Skip(1))
        {
            if (el.Contains(':'))
            {
                ret = false;
                break;
            }                
        }

        return ret;
    }
}