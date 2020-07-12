using System.Reflection;
using System;

namespace RyanLang
{
    class Program
    {
        static void Main(string[] args)
        {
            string pgrm = "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++.";
            var b = Compiler.Compile(BrainFuckToRyanLang.Compile(pgrm), "Hello_world");

            b.Save("Hello_world.exe");

            Console.ReadKey();
        }
    }
}
