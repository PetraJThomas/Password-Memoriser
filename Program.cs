using System;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordMemoriser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string mode = Environment.GetEnvironmentVariable("RUN_MODE") ?? "normal";

            if (mode == "websocket")
            {
                await RunWebSocketMode();
            }
            else
            {
                RunConsoleMode();
            }
        }

        static async Task RunWebSocketMode()
        {
            string url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://*:5000/";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening on " + url);

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = wsContext.WebSocket;
                    await HandleWebSocketConnection(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        static void RunConsoleMode()
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

        static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            var session = new PasswordMemoriserSession();

            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                string responseMessage = session.ProcessInput(receivedMessage);

                byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
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

    public class PasswordMemoriserSession
    {
        private SecureString password;
        private string hashedPassword;
        private int passLength;
        private int correctAttempts;
        private int wrongAttempts;
        private int longestStreak;
        private int currentStreak;
        private long totalTime;
        private bool isFinished;
        private bool isPasswordSet;

        public PasswordMemoriserSession()
        {
            correctAttempts = 0;
            wrongAttempts = 0;
            longestStreak = 0;
            currentStreak = 0;
            totalTime = 0;
            isFinished = false;
            isPasswordSet = false;
        }

        public string ProcessInput(string input)
        {
            if (!isPasswordSet)
            {
                password = new SecureString();
                foreach (char c in input)
                {
                    password.AppendChar(c);
                }
                password.MakeReadOnly();
                hashedPassword = HashPassword(password);
                passLength = password.Length;
                isPasswordSet = true;
                return "Password set! Your password length is " + passLength + " characters long - " + new string('*', passLength) + "\nPress Enter to Continue...\n";
            }

            var stopwatch = Stopwatch.StartNew();
            var comparePass = new SecureString();
            foreach (char c in input)
            {
                comparePass.AppendChar(c);
            }
            comparePass.MakeReadOnly();

            stopwatch.Stop();
            totalTime += stopwatch.ElapsedMilliseconds;

            if (VerifyPassword(comparePass, hashedPassword))
            {
                correctAttempts++;
                currentStreak++;
                if (currentStreak > longestStreak)
                {
                    longestStreak = currentStreak;
                }
                return "\n\nCorrect! Well done.\nPress Enter to Continue... Or type 'finish' and press Enter to wrap up: ";
            }
            else
            {
                wrongAttempts++;
                currentStreak = 0;
                return "\n\nWrong! Try again.\nPress Enter to Continue... Or type 'finish' and press Enter to wrap up: ";
            }
        }

        private string HashPassword(SecureString password)
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

        private bool VerifyPassword(SecureString password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
