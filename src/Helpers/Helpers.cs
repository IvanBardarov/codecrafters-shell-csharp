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
            var c = inputChars[i];

            if (c == '\'' && !isInsideDoubleQuotes)
            {
                isInsideSingleQuotes = !isInsideSingleQuotes;                
            }
            else if(c == '"' && !isInsideSingleQuotes)
            {
                isInsideDoubleQuotes = !isInsideDoubleQuotes;
            }
            else if (char.IsWhiteSpace(c) &&
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
                tmpStr += c;
            }
        }

        if (!string.IsNullOrWhiteSpace(tmpStr))
        {
            ret.Add(tmpStr);
        }
        
        return ret;
    }
}