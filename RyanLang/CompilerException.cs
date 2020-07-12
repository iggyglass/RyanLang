using System;

namespace RyanLang
{
    public class CompilerException : Exception
    {

        public CompilerException() : base() { }

        public CompilerException(string msg) : base(msg) { }
    }
}
