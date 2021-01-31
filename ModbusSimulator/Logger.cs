using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    class Logger
    {
        public static void Log(string message, string severity = "Info", StackFrame frame = null)
        {
            StackFrame theFrame = frame ?? new StackTrace().GetFrame(1);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{DateTime.Now.ToString("HH:mm:ss")}.{DateTime.Now.Millisecond.ToString("D3")} ");

            Console.ForegroundColor = severity switch
            {
                "Info" => ConsoleColor.White,
                "Warn" => ConsoleColor.Yellow,
                "Error" => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
            Console.Write($"[{severity}] ");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"[{theFrame.GetMethod().DeclaringType.Name}.{theFrame.GetMethod().Name}] \t");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }

        public static void Warn(string message) => Log(message, "Warn", new StackTrace().GetFrame(1));
        public static void Error(string message) => Log(message, "Error", new StackTrace().GetFrame(1));
    }
}
