using System;

namespace RyanLang
{
    public class CLIException : Exception
    {

        public CLIException() : base() { }

        public CLIException(string msg) : base(msg) { }
    }
}
