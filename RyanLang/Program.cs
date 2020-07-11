using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RyanLang
{
    class Program
    {
        static void Main(string[] args)
        {
            string pgrm = ",.";
            Compiler.Compile(pgrm);
            Console.ReadKey();
        }
    }
}
