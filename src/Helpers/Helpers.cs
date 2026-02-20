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

        var inputChars = input.ToCharArray().ToList();
        var lastSingleQuoteIndex = inputChars.FindLastIndex(c => c == '\'');
        var isInsideSingleQuotes = false;

        for (var i = 0; i < inputChars.Count; i++)
        {
            var c = inputChars[i];

            if(c == '\'' && i >= lastSingleQuoteIndex)
            {                
                isInsideSingleQuotes = false;
            }
            else if (c == '\'')
            {
                isInsideSingleQuotes = !isInsideSingleQuotes;                
            }
            else if (char.IsWhiteSpace(c) && !isInsideSingleQuotes)
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