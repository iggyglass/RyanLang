using System;
using System.IO;

namespace RyanLang
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Not enough parameters specified.");
                    return;
                }

                Console.WriteLine($"Starting at {getTimestamp(DateTime.Now)}");

                string code = File.ReadAllText(args[0]);
                var program = Compiler.Compile(code, args[1]);

                program.Save(args[1]);

                Console.WriteLine($"Finished at {getTimestamp(DateTime.Now)}");
            }
            catch (CompilerException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Specified application name is invalid.");
            }
            catch (IOException)
            {
                Console.WriteLine("An IO exception occured.");
            }
        }

        private static string getTimestamp(DateTime time)
        {
            return time.ToString("yyyyMMddHHmmssffff");
        }
    }
}
