public static class FlowControlHelpers
{
    public static bool FlowControl(Commands command)
    {
        if (command.Result == "exit")
        {
            return false;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(command.RedirectToFile))
            {
                if (command.Error && string.IsNullOrWhiteSpace(command.Result))
                {
                    if (command.TypeOfOperator == TypeOfOperator.RedirectStdOut)
                        Console.WriteLine(command.ErrorMessage);
                    else if (command.TypeOfOperator == TypeOfOperator.RedirectStdErr)
                        FileHelpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                    else if (command.TypeOfOperator == TypeOfOperator.AppendStdOut)
                    {
                        Console.WriteLine(command.ErrorMessage);
                        FileHelpers.RedirectToFile(string.Empty, command.RedirectToFile, command.TypeOfOperator);
                    }
                    else if (command.TypeOfOperator == TypeOfOperator.AppendStdErr)
                    {
                        FileHelpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                    }
                }
                else if (command.Error && !string.IsNullOrWhiteSpace(command.Result))
                {
                    if (command.TypeOfOperator == TypeOfOperator.RedirectStdOut)
                    {
                        Console.WriteLine(command.ErrorMessage);
                        FileHelpers.RedirectToFile(command.Result ?? "", command.RedirectToFile, command.TypeOfOperator);
                    }
                    else if (command.TypeOfOperator == TypeOfOperator.RedirectStdErr)
                    {
                        Console.WriteLine(command.Result);
                        FileHelpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                    }
                    else if (command.TypeOfOperator == TypeOfOperator.AppendStdOut)
                    {
                        Console.WriteLine(command.ErrorMessage);
                        FileHelpers.RedirectToFile(command.Result ?? "", command.RedirectToFile, command.TypeOfOperator);
                    }
                    else if (command.TypeOfOperator == TypeOfOperator.AppendStdErr)
                    {
                        Console.WriteLine(command.Result);
                        FileHelpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                    }
                }
                else
                {
                    if (command.TypeOfOperator == TypeOfOperator.RedirectStdOut)
                        FileHelpers.RedirectToFile(command.Result ?? "", command.RedirectToFile, command.TypeOfOperator);
                    else if (command.TypeOfOperator == TypeOfOperator.RedirectStdErr)
                    {
                        Console.WriteLine(command.Result);
                        FileHelpers.RedirectToFile(string.Empty, command.RedirectToFile, command.TypeOfOperator);
                    }
                    else if (command.TypeOfOperator == TypeOfOperator.AppendStdOut)
                    {
                        if (command.Error)
                            FileHelpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                        else
                            FileHelpers.RedirectToFile(command.Result ?? "", command.RedirectToFile, command.TypeOfOperator);
                    }
                    else if (command.TypeOfOperator == TypeOfOperator.AppendStdErr)
                    {
                        if (command.Error)
                            FileHelpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                        else
                        {
                            Console.WriteLine(command.Result);
                            FileHelpers.RedirectToFile(string.Empty ?? "", command.RedirectToFile, command.TypeOfOperator);
                        }
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(command.Result))
            {
                Console.WriteLine(string.Join(" ", command.Result));
            }
            else if (!string.IsNullOrWhiteSpace(command.ErrorMessage))
            {
                Console.WriteLine(string.Join(" ", command.ErrorMessage));
            }
        }

        return true;
    }
}