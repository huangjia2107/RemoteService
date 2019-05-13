using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Models;

namespace Server.Utils
{
    static class ConsoleHelper
    {
        public static void Mark(string message)
        {
            PrintMessage(MessageType.Mark, message);
        }

        public static void Info(string message)
        {
            PrintMessage(MessageType.Info, message);
        }

        public static void Warn(string message)
        {
            PrintMessage(MessageType.Warn, message);
        }

        public static void Error(string message)
        {
            PrintMessage(MessageType.Error, message);
        }

        private static void PrintMessage(MessageType type, string message)
        {
            switch (type)
            {
                case MessageType.Mark:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case MessageType.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine(string.Format("{0} {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), type, message));

            Console.ResetColor();
        }
    }
}
