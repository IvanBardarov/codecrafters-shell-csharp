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
}