class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");
            
            string? consoleInput = Console.ReadLine();
            string userInput = string.Empty;

            if (!string.IsNullOrWhiteSpace(consoleInput))
            {
                userInput = consoleInput;
            }

            var command = new Commands(userInput);

            if(command.Result == "exit")
            {
                break;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(command.RedirectToFile))
                {                    
                    if (command.Error && string.IsNullOrWhiteSpace(command.Result))
                    {
                        if(command.TypeOfOperator == TypeOfOperator.RedirectStdOut)
                            Console.WriteLine(command.ErrorMessage);
                        else if(command.TypeOfOperator == TypeOfOperator.RedirectStdErr)                        
                            Helpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                        else if(command.TypeOfOperator == TypeOfOperator.AppendStdOut)
                        {
                            Console.WriteLine(command.ErrorMessage);
                            Helpers.RedirectToFile(string.Empty, command.RedirectToFile, command.TypeOfOperator);
                        }  
                        else if(command.TypeOfOperator == TypeOfOperator.AppendStdErr)
                        {
                            Helpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                        }                         
                    }
                    else if(command.Error && !string.IsNullOrWhiteSpace(command.Result))
                    {
                        if(command.TypeOfOperator == TypeOfOperator.RedirectStdOut)
                        {
                            Console.WriteLine(command.ErrorMessage);
                            Helpers.RedirectToFile(command.Result ?? "", command.RedirectToFile, command.TypeOfOperator);
                        }
                        else if(command.TypeOfOperator == TypeOfOperator.RedirectStdErr)
                        {
                            Console.WriteLine(command.Result);
                            Helpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                        }
                        else if(command.TypeOfOperator == TypeOfOperator.AppendStdOut)
                        {
                            Console.WriteLine(command.ErrorMessage);
                            Helpers.RedirectToFile(command.Result ?? "", command.RedirectToFile, command.TypeOfOperator);
                        }  
                        else if(command.TypeOfOperator == TypeOfOperator.AppendStdErr)
                        {
                            Console.WriteLine(command.Result);
                            Helpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                        }  
                    }
                    else
                    {
                        if(command.TypeOfOperator == TypeOfOperator.RedirectStdOut)
                            Helpers.RedirectToFile(command.Result ?? "", command.RedirectToFile, command.TypeOfOperator);
                        else if(command.TypeOfOperator == TypeOfOperator.RedirectStdErr)
                        {
                            Console.WriteLine(command.Result);
                            Helpers.RedirectToFile(string.Empty, command.RedirectToFile, command.TypeOfOperator);
                        }    
                        else if(command.TypeOfOperator == TypeOfOperator.AppendStdOut)
                        {
                            if(command.Error)
                                Helpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                            else
                                Helpers.RedirectToFile(command.Result ?? "", command.RedirectToFile, command.TypeOfOperator);
                        } 
                        else if(command.TypeOfOperator == TypeOfOperator.AppendStdErr)
                        {
                            if(command.Error)
                                Helpers.RedirectToFile(command.ErrorMessage ?? "", command.RedirectToFile, command.TypeOfOperator);
                            else
                            {
                                Console.WriteLine(command.Result);
                                Helpers.RedirectToFile(string.Empty ?? "", command.RedirectToFile, command.TypeOfOperator);
                            }                                
                        }                                                   
                    }
                }
                else if(!string.IsNullOrWhiteSpace(command.Result))
                {
                    Console.WriteLine(string.Join(" ", command.Result));
                } 
                else if(!string.IsNullOrWhiteSpace(command.ErrorMessage))
                {
                    Console.WriteLine(string.Join(" ", command.ErrorMessage));
                }
            }           
        }
    }
}