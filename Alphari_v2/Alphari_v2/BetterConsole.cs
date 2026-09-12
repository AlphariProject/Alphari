// using Console = Cosmos.System.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System;
using System.Threading;
using Console = System.Console;
using System.Runtime.Serialization;

namespace Alphari_v2
{
    public class BetterConsole
    {
        //Console Console;
        public BetterConsole(/*Console service*/)
        {
            try
            {
                //Console = service;
            }
            catch
            {
                System.Console.WriteLine("Service error: BC Init fail!");
            }
        }

        public void Write(string message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == '\\' && i + 1 < message.Length)
                {
                    char nextChar = message[i + 1];
                    if (nextChar == 'w' || nextChar == 'r' || nextChar == 'g' || nextChar == 'b' ||
                        nextChar == 'y' || nextChar == 'c' || nextChar == 'm' || nextChar == 'n')
                    {
                        switch (nextChar)
                        {
                            case 'w': Console.ForegroundColor = ConsoleColor.White; break;
                            case 'r': Console.ForegroundColor = ConsoleColor.Red; break;
                            case 'g': Console.ForegroundColor = ConsoleColor.Green; break;
                            case 'b': Console.ForegroundColor = ConsoleColor.Blue; break;
                            case 'y': Console.ForegroundColor = ConsoleColor.Yellow; break;
                            case 'c': Console.ForegroundColor = ConsoleColor.Cyan; break;
                            case 'm': Console.ForegroundColor = ConsoleColor.Magenta; break;
                            case 'n': Console.ForegroundColor = ConsoleColor.Gray; break;
                        }
                        i++;
                        continue;
                    }
                }
                Console.Write(message[i]);
            }
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void WriteLine(string message)
        {
            Write(message + '\n');
        }

        public KeyEvent ReadKey()
        {
            return KeyboardManager.ReadKey();
        }

        public string ReadLine()
        {
            string msg = System.Console.ReadLine();
            Write("\n\r");
            return msg;
        }

        public (ConsoleColor, ConsoleColor) GetColors()
        {
            return (Console.ForegroundColor, Console.BackgroundColor);
        }

        public void SetColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public void SetBkColor(ConsoleColor color)
        {
            Console.BackgroundColor = color;
        }
    }

    public static class VgaMapper
    {
        private static readonly Dictionary<char, byte> _map = new Dictionary<char, byte>()
        {
            { '█', 0xDB }, { '▄', 0xDC }, { '▀', 0xDF }, { '▒', 0xB1 }, { '▓', 0xB2 },
            { '╔', 0xC9 }, { '╗', 0xBB }, { '╚', 0xC8 }, { '╝', 0xBC },
            { '═', 0xCD }, { '║', 0xBA }, { '╠', 0xCC }, { '╣', 0xB9 },
            { '╦', 0xCB }, { '╩', 0xCA }, { '╬', 0xCE },
            { '┌', 0xDA }, { '┐', 0xBF }, { '└', 0xC0 }, { '┘', 0xD9 },
            { '─', 0xC4 }, { '│', 0xB3 },
            { '©', 0xB8 }, { '°', 0xF8 }, { '·', 0xFA }, { '•', 0x07 }
        };

        public static byte ToVgaByte(char c)
        {
            if (_map.ContainsKey(c)) return _map[c];
            if (c <= 127) return (byte)c;
            return 0x20;
        }
    }
}
