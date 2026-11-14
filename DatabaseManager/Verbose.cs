using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseManager
{
    public static class Verbose
    {
        public static bool IsVerbose { get; set; }
        public static void Prt(string sender, object text, bool noWriteLine = false)
        {
            if (IsVerbose)
            {
                DateTime it = DateTime.UtcNow;
                string po = $"{it.Year:D4}.{it.Month:D2}.{it.Day:D2} UTC {it.Hour:D2}:{it.Minute:D2}:{it.Second:D2}.{it.Microsecond:D3} | <{sender.ToUpper()}> {text}";
                if (noWriteLine)
                {
                    Console.Write(po);
                }
                else
                    Console.WriteLine(po);
            }
        }
        public static void PrtNoTime(object text, bool noWriteLine = false)
        {
            if (IsVerbose)
            {
                if (noWriteLine) Console.Write(text);
                else Console.WriteLine(text);
            }
        }
        public static string? Input(string sender, string question, bool isRequired)
        {
            if (IsVerbose)
            {
                Console.WriteLine($"<{sender.ToUpper()}> {question}\n");
                string? output;
                if (isRequired)
                {
                    do
                    {
                        Console.CursorTop--;
                        Console.CursorLeft = 0;
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                        Console.CursorLeft = 0;
                        Console.Write("> ");
                        output = Console.ReadLine();
                    } while (string.IsNullOrWhiteSpace(output));
                }
                else
                {
                    Console.CursorTop--;
                    Console.CursorLeft = 0;
                    Console.Write("> ");
                    output = Console.ReadLine();
                }
                return output;
            }
            else return null;
        }
    }
}
