using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RyanLang
{
    public static class Compiler
    {

        public static AssemblyBuilder Compile(string code, string name)
        {
            return compileCode(cleanInput(code), name);
        }

        private static string cleanInput(string code)
        {
            string output = "";

            // Make input string only consist of ( and )
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] != '(' && code[i] != ')') continue;

                output += code[i];
            }

            // Make sure code is of a valid length
            if (output.Length % 3 != 0) throw new CompilerException("Code is of an invalid length.");

            return output;
        }

        private static AssemblyBuilder compileCode(string code, string name)
        {
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Save);
            ModuleBuilder module = assembly.DefineDynamicModule(name, name + ".exe");
            TypeBuilder type = module.DefineType("Program", TypeAttributes.Public);
            MethodBuilder method = type.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, null, null);
            ILGenerator gen = method.GetILGenerator();

            ReadOnlySpan<char> chars = code.AsSpan();

            gen.DeclareLocal(typeof(int)); // Tape pointer
            gen.DeclareLocal(typeof(char[])); // Tape
            gen.DeclareLocal(typeof(ConsoleKeyInfo)); // Used for reading chars into

            // Key corresponds to the index of the open/close char
            Dictionary<int, Label> labels = new Dictionary<int, Label>();

            // Push 0 onto stack then pop it into integer
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Stloc_0);

            // Push 65535 onto stack then use it to allocate new char array
            gen.Emit(OpCodes.Ldc_I4, 65535);
            gen.Emit(OpCodes.Newarr, typeof(char));
            gen.Emit(OpCodes.Stloc_1);

            for (int i = 0; i < chars.Length; i += 3)
            {
                string current = chars.Slice(i, 3).ToString();

                switch (current)
                {
                    case "(((": // > in brainfuck
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldc_I4_1);
                        gen.Emit(OpCodes.Add);
                        gen.Emit(OpCodes.Stloc_0);
                        break;
                    case "(()": // < in brainfuck
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldc_I4_1);
                        gen.Emit(OpCodes.Sub);
                        gen.Emit(OpCodes.Stloc_0);
                        break;
                    case "()(": // + in brainfuck
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
                    case "())": // - in brainfuck
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
                    case ")((": // . in brainfuck
                        gen.Emit(OpCodes.Ldloc_1);
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldelem_U2);
                        gen.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(char) }));
                        break;
                    case ")()": // , in brainfuck
                        gen.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadKey", new Type[] { }));
                        gen.Emit(OpCodes.Stloc_2);
                        gen.Emit(OpCodes.Ldloc_1);
                        gen.Emit(OpCodes.Ldloc_0);
                        gen.Emit(OpCodes.Ldloca_S, (byte)2);
                        gen.Emit(OpCodes.Call, typeof(ConsoleKeyInfo).GetMethod("get_KeyChar", new Type[] { }));
                        gen.Emit(OpCodes.Stelem_I2);
                        break;
                    case "))(": // [ in brainfuck
                        {
                            int end = findClosing(chars, i + 3);

                            labels.Add(i, gen.DefineLabel());
                            labels.Add(end, gen.DefineLabel());

                            gen.Emit(OpCodes.Br, labels[end]);

                            gen.MarkLabel(labels[i]);
                        }
                        break;
                    case ")))": // ] in brainfuck
                        {
                            int start = findOpening(chars, i - 3);

                            gen.MarkLabel(labels[i + 3]);

                            gen.Emit(OpCodes.Ldloc_1);
                            gen.Emit(OpCodes.Ldloc_0);
                            gen.Emit(OpCodes.Ldelem_U2);
                            gen.Emit(OpCodes.Ldc_I4_0);
                            gen.Emit(OpCodes.Cgt_Un);
                            gen.Emit(OpCodes.Brtrue, labels[start]);
                        }
                        break;
                }
            }

            gen.Emit(OpCodes.Ret);

            type.CreateType();
            assembly.SetEntryPoint(method.GetBaseDefinition(), PEFileKinds.ConsoleApplication);

            return assembly;
        }

        private static int findOpening(ReadOnlySpan<char> input, int end)
        {
            int counter = 1;

            while (counter > 0)
            {
                if (end == -1) throw new CompilerException("Mismatched brackets.");

                string current = input.Slice(end - 3, 3).ToString();

                if (current == "))(") counter--; // open bracket
                else if (current == ")))") counter++; // close bracket

                end -= 3;
            }

            return end;
        }

        private static int findClosing(ReadOnlySpan<char> input, int start)
        {
            int counter = 1;

            while (counter > 0)
            {
                if (start == input.Length) throw new CompilerException("Mismatched brackets.");

                string current = input.Slice(start, 3).ToString();

                if (current == "))(") counter++; // open bracket
                else if (current == ")))") counter--; // close bracket

                start += 3;
            }

            return start;
        }
    }
}
