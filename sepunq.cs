using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sepunq
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                invalid();
            }
            else if (args[0] == "help" || args[0] == "/?" || args[0] == "?" || args[0] == "-h" || args[0] == "--help") 
            {
                help();
            }
            else
            {
                using(FileStream f = new FileStream(args[0], FileMode.Open, FileAccess.Read))
                using (BinaryReader binaryReader = new BinaryReader(f))
                {
                    f.Seek(0x0, SeekOrigin.Begin);
                    ushort offset = binaryReader.ReadUInt16();
                    f.Seek(offset, SeekOrigin.Begin);
                    int len = (int)f.Length;
                    var bytes = binaryReader.ReadBytes(len);
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        bytes[i] ^= 90;
                    }
                    System.IO.File.WriteAllBytes(args[1], bytes);
                }
            }
        }
        static void help()
        {
            Console.WriteLine("  ___  ___ _ __  _   _ _ __   __ _ ");
            Console.WriteLine(" / __|/ _ \\ '_ \\| | | | '_ \\ / _` |");
            Console.WriteLine(" \\__ \\  __/ |_) | |_| | | | | (_| |");
            Console.WriteLine(" |___/\\___| .__/ \\__,_|_| |_|\\__, |");
            Console.WriteLine("          | |                   | |");
            Console.WriteLine("          |_|                   |_|");
            Console.WriteLine("\nDecrypt/Extract files quarantined by SEP");
            Console.WriteLine("Author: James Haughom Jr");
            Console.Write("Command Format: sepunq.exe quarantined.VBN outfile.bin");

        }
        static void invalid()
        {
            Console.WriteLine("\nInvalid parameters provided!\n\n");
            help();
            Console.WriteLine("\n\nPress any key to exit: ");
            Console.ReadKey();
        }
    }
}
