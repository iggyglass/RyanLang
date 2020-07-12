namespace RyanLang
{
    public static class BrainFuckToRyanLang
    {
        public static string Compile(string code)
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
                        validateEnd(code, i + 1);
                        output += "))(";
                        break;
                    case ']':
                        validateStart(code, i - 1);
                        output += ")))";
                        break;
                    default:
                        continue;
                }
            }

            return output;
        }

        private static void validateStart(string input, int end)
        {
            int count = 1;

            while (count > 0)
            {
                if (end == -1) throw new CompilerException("Mismatched brackets.");

                if (input[end] == '[') count--;
                else if (input[end] == ']') count++;

                end--;
            }
        }

        private static void validateEnd(string input, int start)
        {
            int count = 1;

            while (count > 0)
            {
                if (start == input.Length) throw new CompilerException("Mismatched brackets.");

                if (input[start] == '[') count++;
                else if (input[start] == ']') count--;

                start++;
            }
        }
    }
}
