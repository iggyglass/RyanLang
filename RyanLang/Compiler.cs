using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RyanLang
{
    public static class Compiler
    {

        public static void Compile(string code)
        {
            DynamicMethod method = new DynamicMethod("Rlbf", null, new Type[] { });
            ILGenerator gen = method.GetILGenerator();

            gen.DeclareLocal(typeof(int)); // Tape pointer
            gen.DeclareLocal(typeof(char[])); // Tape
            gen.DeclareLocal(typeof(ConsoleKeyInfo)); // Used for reading chars into

            // Key corresponds to the index of the current char
            Dictionary<int, Label> labels = new Dictionary<int, Label>();

            // Push 0 onto stack then pop it into integer
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Stloc_0);

            // Push 65535 onto stack then use it to allocate new char array
            gen.Emit(OpCodes.Ldc_I4, 65535);
            gen.Emit(OpCodes.Newarr, typeof(char));
            gen.Emit(OpCodes.Stloc_1);

            for (int i = 0; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case '>':
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldc_I4_1);
                        gen.Emit(OpCodes.Add);
                        gen.Emit(OpCodes.Stloc_0);
                        break;
                    case '<':
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldc_I4_1);
                        gen.Emit(OpCodes.Sub);
                        gen.Emit(OpCodes.Stloc_0);
                        break;
                    case '+':
                        gen.Emit(OpCodes.Ldloc_1);
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldelema, typeof(char));
                        gen.Emit(OpCodes.Dup);
                        gen.Emit(OpCodes.Ldind_U2);
                        gen.Emit(OpCodes.Ldc_I4_1);
                        gen.Emit(OpCodes.Add);
                        gen.Emit(OpCodes.Conv_U2);
                        gen.Emit(OpCodes.Stind_I2);
                        break;
                    case '-':
                        gen.Emit(OpCodes.Ldloc_1);
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldelema, typeof(char));
                        gen.Emit(OpCodes.Dup);
                        gen.Emit(OpCodes.Ldind_U2);
                        gen.Emit(OpCodes.Ldc_I4_1);
                        gen.Emit(OpCodes.Sub);
                        gen.Emit(OpCodes.Conv_U2);
                        gen.Emit(OpCodes.Stind_I2);
                        break;
                    case '.':
                        gen.Emit(OpCodes.Ldloc_1);
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldelem_U2);
                        gen.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(char) }));
                        break;
                    case ',':
                        gen.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadKey", new Type[] { }));
                        gen.Emit(OpCodes.Stloc_2);
                        gen.Emit(OpCodes.Ldloc_1);
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldloca_S, (byte)2);
                        gen.Emit(OpCodes.Call, typeof(ConsoleKeyInfo).GetMethod("get_KeyChar", new Type[] { }));
                        gen.Emit(OpCodes.Stelem_I2);
                        break;
                    case '[':
                        {
                            int end = findClosing(code, i + 1);

                            labels.Add(i, gen.DefineLabel());
                            labels.Add(end, gen.DefineLabel());

                            gen.Emit(OpCodes.Br, labels[end]);

                            gen.MarkLabel(labels[i]);
                        }
                        break;
                    case ']':
                        {
                            int start = findOpening(code, i - 1);

                            gen.MarkLabel(labels[i + 1]);

                            gen.Emit(OpCodes.Ldloc_1);
                            gen.Emit(OpCodes.Ldloc_0);
                            gen.Emit(OpCodes.Ldelem_U2);
                            gen.Emit(OpCodes.Ldc_I4_0);
                            gen.Emit(OpCodes.Cgt_Un);
                            gen.Emit(OpCodes.Brtrue, labels[start + 1]);
                        }
                        break;
                }
            }

            gen.Emit(OpCodes.Ret);

            // FOR TESTING:
            method.Invoke(null, new object[] { });
        }

        private static int findOpening(string input, int end)
        {
            int counter = 1;

            while (counter > 0)
            {
                if (input[end] == '[') counter--;
                else if (input[end] == ']') counter++;

                end--;
            }

            return end;
        }

        private static int findClosing(string input, int start)
        {
            int counter = 1;

            while (counter > 0)
            {
                if (input[start] == '[') counter++;
                else if (input[start] == ']') counter--;

                start++;
            }

            return start;
        }
    }
}
