using System;
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;

namespace PasswordMemoriser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Password Memoriser!");

            Console.Write("Type in the password you want to memorize: ");
            SecureString password = MaskInput();
            if (password.Length == 0)
            {
                Console.WriteLine("You didn't input a Password...");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            string hashedPassword = HashPassword(password);
            int passLength = password.Length;

            Console.Clear();
            string characterText = passLength == 1 ? "character" : "characters";
            Console.Write($"Your password length is {passLength} {characterText} long - ");
            Console.WriteLine(new string('*', passLength));

            Console.WriteLine("\nPress Enter to Continue...");
            Console.ReadKey();

            int correctAttempts = 0;
            int wrongAttempts = 0;
            int longestStreak = 0;
            int currentStreak = 0;
            long totalTime = 0;
            bool isFinished = false;

            while (!isFinished)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Console.Write("\nEnter the password again to see if you've typed it in correctly: ");
                SecureString comparePass = MaskInput();

                stopwatch.Stop();
                totalTime += stopwatch.ElapsedMilliseconds;

                if (VerifyPassword(comparePass, hashedPassword))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n\nCorrect! Well done.\n");
                    correctAttempts++;
                    currentStreak++;
                    if (currentStreak > longestStreak)
                    {
                        longestStreak = currentStreak;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\nWrong! Try again.\n");
                    wrongAttempts++;
                    currentStreak = 0;
                }
                Console.ResetColor();

                Console.Write("\nPress Enter to Continue... Or type 'finish' and press Enter to wrap up: ");
                string finish = Console.ReadLine();
                if (finish.Equals("finish", StringComparison.OrdinalIgnoreCase))
                {
                    isFinished = true;
                }
            }

            int totalAttempts = correctAttempts + wrongAttempts;
            double accuracyCorrectPercent = Math.Round(((double)correctAttempts / totalAttempts) * 100, 2);
            double accuracyWrongPercent = Math.Round(((double)wrongAttempts / totalAttempts) * 100, 2);
            double averageTimePerAttempt = Math.Round((double)totalTime / totalAttempts / 1000, 2); // Convert to seconds

            Console.Write($"\nYou got your Password {new string('*', passLength)} Correct {correctAttempts} times, and Wrong {wrongAttempts} times | Accuracy: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{accuracyCorrectPercent}% Correct");
            Console.ResetColor();
            Console.Write(", and ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{accuracyWrongPercent}% Wrong...");
            Console.ResetColor();

            // Display additional metrics
            Console.Write("\n\nAverage Time per Attempt: ");
            if (averageTimePerAttempt < 10)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write($"{averageTimePerAttempt} seconds");
            Console.ResetColor();

            Console.Write("\nLongest Streak of Correct Attempts: ");
            if (longestStreak >= 7)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine($"{longestStreak}");
            Console.ResetColor();

            Console.WriteLine("\n\nThank you for using Password Memoriser. Press any key to exit...");
            Console.ReadKey();
        }

        static SecureString MaskInput()
        {
            SecureString secureString = new SecureString();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    secureString.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && secureString.Length > 0)
                {
                    secureString.RemoveAt(secureString.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            secureString.MakeReadOnly();
            return secureString;
        }

        static string HashPassword(SecureString password)
        {
            IntPtr passwordBSTR = IntPtr.Zero;
            try
            {
                passwordBSTR = Marshal.SecureStringToBSTR(password);
                string passwordString = Marshal.PtrToStringBSTR(passwordBSTR);

                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(passwordString));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            finally
            {
                if (passwordBSTR != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(passwordBSTR);
                }
            }
        }

        static bool VerifyPassword(SecureString password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}