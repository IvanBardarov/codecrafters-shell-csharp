public static class Helpers
{
    public static List<string> ParseUserInput(string command, string input)
    {
        var ret = new List<string>();
        var tmpStr = string.Empty;

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
                    tmpStr += inputChars[i];
                }
                else
                {
                    tmpStr += currentChar;
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
                if (!string.IsNullOrWhiteSpace(tmpStr))
                {
                    ret.Add(tmpStr);
                    tmpStr = string.Empty;
                }
            }
            else
            {
                tmpStr += currentChar;
            }
        }

        if (!string.IsNullOrWhiteSpace(tmpStr))
        {
            ret.Add(tmpStr);
        }
        
        return ret;
    }
}