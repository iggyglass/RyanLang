using System;
using System.IO;

namespace RyanLang
{
    public class Program
    {

        private enum Settings
        {
            Normal,
            BrainfuckToRyan,
            BrainfuckCompile
        }

        private static Settings settings = Settings.Normal;

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Not enough parameters specified.");
                    return;
                }

                if (args.Length >= 3) parseSettings(args[2]);

                Console.WriteLine($"Starting at {getTimestamp(DateTime.Now)}");

                string code = File.ReadAllText(args[0]);
                code = settings != Settings.Normal ? BrainFuckToRyanLang.Compile(code) : code;

                if (settings != Settings.BrainfuckToRyan)
                {
                    var program = Compiler.Compile(code, args[1]);

                    program.Save(args[1] + ".exe");
                }
                else
                {
                    if (!File.Exists(args[1])) File.Create(args[1]);

                    File.WriteAllText(args[1], code);
                }

                Console.WriteLine($"Finished at {getTimestamp(DateTime.Now)}");
            }
            catch (CompilerException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (CLIException e)
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

        private static void parseSettings(string input)
        {
            input = input.ToLower();

            if (input == "-bf") settings = Settings.BrainfuckToRyan;
            else if (input == "-bfc") settings = Settings.BrainfuckCompile;
            else throw new CLIException($"Unable to parse parameter {input}.");
        }

        private static string getTimestamp(DateTime time)
        {
            return time.ToString("yyyyMMddHHmmssffff");
        }
    }
}
