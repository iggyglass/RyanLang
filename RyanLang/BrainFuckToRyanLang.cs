namespace RyanLang
{
    public static class BrainFuckToRyanLang
    {
        public static string Compile(string code) // Perhaps do some validation on input -- later
        {
            string output = "";

            for (int i = 0; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case '>':
                        output += "(((";
                        break;
                    case '<':
                        output += "(()";
                        break;
                    case '+':
                        output += "()(";
                        break;
                    case '-':
                        output += "())";
                        break;
                    case '.':
                        output += ")((";
                        break;
                    case ',':
                        output += ")()";
                        break;
                    case '[':
                        output += "))(";
                        break;
                    case ']':
                        output += ")))";
                        break;
                    default:
                        continue;
                }
            }

            return output;
        }
    }
}
