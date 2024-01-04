using System;
using ConsolePasswordMasker;
    namespace PasswordMem
    {
        class Program
        {
            static PasswordMasker masker = new PasswordMasker();
            static PasswordMasker masker2 = new PasswordMasker();
            static void Main(string[] args)
            {

               string Pass = masker.Mask(loginText:"Type in the password you want to memorise: ", charMask:'*', useBeep: false);
               int PassLength = Pass.Length;
                Console.Clear();
                if (PassLength == 0)
                {
                    Console.Write("You didn't input a Password...");
                    Console.Write(" Press any key to continue...");
                    Console.Read();
                    Environment.Exit(0); 
                }
                Console.Write($"Your password length is {PassLength} characters long - ");
                for (int i= 0; PassLength > i; i++)
                {
                    Console.Write("*");
                }
                Console.Write("\n\nPress Enter to Continue...");
                Console.ReadKey();             
                int loop =-1;
                int c = 0;
                int w = 0;
                while(loop != 0)
                {
                    string comparePass = masker2.Mask(loginText:"Enter the password again to see if you've typed it in correctly: ", charMask:'*', useBeep: false);
                    if(comparePass == Pass)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nCorrect!\n");
                        Console.ResetColor();
                        c++;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nWrong!\n");
                        Console.ResetColor();
                        w++;

                    }
                    Console.Write("Press Enter to Continue... Or type Finish and press Enter to wrap up: ");
                    string finish = Console.ReadLine();
                    if (finish.ToLower() == "finish")
                    {
                        loop = 0;
                    }
                }

                Console.Write($"\nYou got your Passsword ");

                for (int i= 0; PassLength > i; i++)
                {
                    Console.Write("*");
                }
                string t = "";
               
                if (c == 1 || w == 1)
                {
                    t = "time";
                }

                if (c > 1 && w > 1)
                {
                    t = "times";
                }

                int total = c + w;
                double accuraryCorrectAttempts = c;
                double accuracyCorrectTotalPercent = 0.0;
                double accuracyTotalAttempts = total;
                double accuracyWrongTotalPercent = 0.0;
                double accuracyWrongAttempts = w;
                

                accuracyCorrectTotalPercent = Math.Round(((accuraryCorrectAttempts / accuracyTotalAttempts) * 100),2);
                accuracyWrongTotalPercent = Math.Round(((accuracyWrongAttempts / accuracyTotalAttempts) * 100),2);
                              
                Console.Write($" Correct ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{c} ");
                Console.ResetColor();
                
                if (c == 0)
                {
                    t = "times";
                    Console.Write($"{t}, and Wrong ");
                }
                else if (c == 1)
                {
                    t = "time";
                    Console.Write($"{t}, and Wrong ");
                }
                else
                {
                    t = "times";
                    Console.Write($"{t}, and Wrong ");
                }
                if(accuracyWrongAttempts == 0 && accuracyWrongTotalPercent == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write($"{w} ");
                Console.ResetColor();
                if (w == 0)
                {
                    t = "times";
                    Console.Write($"{t} | Accuracy: ");
                }
                else if (w == 1)
                {
                    t = "time";
                    Console.Write($"{t} | Accuracy: ");
                }
                else
                {
                    t = "times";
                    Console.Write($"{t} | Accuracy: ");
                }

                if (accuracyCorrectTotalPercent >= 50)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
            
                Console.Write($"{accuracyCorrectTotalPercent}% ");
                Console.ResetColor();
                Console.Write($"Correct and ");
                if(accuracyWrongAttempts == 0 && accuracyWrongTotalPercent == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write($"{accuracyWrongTotalPercent}% ");
                Console.ResetColor();
                if(accuracyWrongAttempts == 0 && accuracyWrongTotalPercent == 0)
                {
                    Console.Write($"Wrong!");
                }
                else
                {
                    Console.Write($"Wrong...");
                }
                Console.WriteLine("\n\nPress any key to Continue...");
                Console.Read();
            }
        }
    }
