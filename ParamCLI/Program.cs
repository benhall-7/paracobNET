using System;
using paracobNET;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ParamCLI
{
    class Program
    {
        const string HelpText =
            "ParamCLI: Edit param files on the command line\n" +
            "required: [input file]\n" +
            "optional: -h / -help ; -o [output] ; -l [label file]";

        static ParamFile paramFile { get; set; }
        static string paramInput { get; set; }
        static string paramOutput { get; set; }
        static Dictionary<ulong, string> hashToString { get; set; }
        static Dictionary<string, ulong> stringToHash { get; set; }
        static Stopwatch stopwatch { get; set; }

        static Stack<IParam> stack { get; set; }
        static bool edited { get; set; } = false;

        static void Main(string[] args)
        {
            stopwatch = new Stopwatch();
            hashToString = new Dictionary<ulong, string>();
            stringToHash = new Dictionary<string, ulong>();
            bool helpPrinted = false;
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-h":
                        case "-help":
                            Console.WriteLine(HelpText);
                            helpPrinted = true;
                            break;
                        case "-o":
                            paramOutput = args[++i];
                            break;
                        case "-l":
                            string name = args[++i];
                            hashToString = LabelIO.GetHashStringDict(name);
                            stringToHash = LabelIO.GetStringHashDict(name);
                            break;
                        default:
                            paramInput = args[i];
                            break;
                    }
                }
                if (paramInput == null)
                {
                    if (!helpPrinted)
                        Console.WriteLine(HelpText);
                    return;
                }
                Console.WriteLine("Initializing...");
                stopwatch.Start();
                paramFile = new ParamFile(paramInput);
                stopwatch.Stop();
                Console.WriteLine("File loaded in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                stack = new Stack<IParam>();
                stack.Push(paramFile.Root);

                EvalUserInput();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                if (!helpPrinted)
                    Console.WriteLine(HelpText);
            }
        }

        static void EvalUserInput()
        {
            while (true)
            {
                string[] commands = Console.ReadLine().Split(';');
                if (EvalGlobal(commands) == EvalResult.Quit)
                    return;
            }
        }

        static EvalResult EvalGlobal(string[] commands)
        {
            for (int i = 0; i < commands.Length; i++)
            {
                stopwatch.Reset();
                string[] args = commands[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "h":
                        case "help":
                            PrintCommands();
                            break;
                        case "q":
                            if (edited)
                            {
                                Console.WriteLine("The file has unsaved changes. Do you want to quit? (n = no, any other = yes)");
                                if (Console.ReadKey().KeyChar == 'n')
                                    break;
                            }
                            return EvalResult.Quit;
                        case "s":
                            if (args.Length > 1)
                                paramOutput = args[1];
                            while (string.IsNullOrEmpty(paramOutput))
                            {
                                Console.WriteLine("Set the desired filename:");
                                paramOutput = Console.ReadLine();
                            }
                            stopwatch.Start();
                            paramFile.Save(paramOutput);
                            stopwatch.Stop();
                            Console.WriteLine("File saved in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                            break;
                        case "so":
                            stopwatch.Start();
                            paramFile.Save(paramInput);
                            stopwatch.Stop();
                            Console.WriteLine("File saved in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                            break;
                        case "b":
                            if (stack.Count > 1)
                                stack.Pop();
                            break;
                        case "pp":
                            {
                                IParam[] path = stack.ToArray();
                                for (int j = 0; j < path.Length; j++)
                                {
                                    string space = "";
                                    for (int spaceCount = 0; spaceCount < j; spaceCount++)
                                        space += " ";
                                    Console.WriteLine(space + ">" + ParamInfo(path[j]));
                                }
                            }
                            break;
                        default:
                            if (EvalCurrentParam(args) == EvalResult.Fail)
                                return EvalResult.Fail;
                            break;
                    }
                }
            }
            return EvalResult.Normal;
        }

        static EvalResult EvalCurrentParam(string[] args)
        {
            IParam current = stack.Peek();
            switch (current.TypeKey)
            {
                case ParamType.@struct:
                    return EvalCommandForStruct(args);
                case ParamType.list:
                    return EvalCommandForArray(args);
                default:
                    return EvalCommandForValue(args);
            }
        }

        static EvalResult EvalCommandForStruct(string[] args)
        {
            ParamStruct paramStruct = stack.Peek() as ParamStruct;
            switch (args[0])
            {
                case "pc":
                    foreach (var member in paramStruct.Nodes)
                        Console.WriteLine($" >{Hash40Util.FormatToString(member.Key, hashToString)}: {ParamInfo(member.Value)}");
                    break;
                case "f":
                    try
                    {
                        ulong value;
                        if (args[1].StartsWith("0x"))
                            value = ulong.Parse(args[1].Substring(2), NumberStyles.HexNumber);
                        else
                            value = Hash40Util.StringToHash40(args[1]);
                        stack.Push(paramStruct.Nodes[value]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return EvalResult.Quit;
                    }
                    break;
                default:
                    return Default(args[0]);
            }
            return EvalResult.Normal;
        }

        static EvalResult EvalCommandForArray(string[] args)
        {
            ParamList paramArray = stack.Peek() as ParamList;
            switch (args[0])
            {
                case "cc":
                    Console.WriteLine(paramArray.Nodes.Count);
                    break;
                case "f":
                    try
                    {
                        stack.Push(paramArray.Nodes[int.Parse(args[1])]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return EvalResult.Fail;
                    }
                    break;
                case "iter":
                    foreach (IParam param in paramArray.Nodes)
                    {

                    }
                    break;
                default:
                    return Default(args[0]);
            }
            return EvalResult.Normal;
        }

        static EvalResult EvalCommandForValue(string[] args)
        {
            ParamValue paramValue = stack.Peek() as ParamValue;
            switch (args[0])
            {
                case "=":
                    try
                    {
                        SetParamValue(paramValue, args[1]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return EvalResult.Fail;
                    }
                    break;
                default:
                    return Default(args[0]);
            }
            return EvalResult.Normal;
        }

        static void SetParamValue(ParamValue param, string str)
        {
            switch (param.TypeKey)
            {
                case ParamType.@bool:
                    param.Value = bool.Parse(str);
                    break;
                case ParamType.@sbyte:
                    param.Value = sbyte.Parse(str);
                    break;
                case ParamType.@byte:
                    param.Value = byte.Parse(str);
                    break;
                case ParamType.@short:
                    param.Value = short.Parse(str);
                    break;
                case ParamType.@ushort:
                    param.Value = ushort.Parse(str);
                    break;
                case ParamType.@int:
                    param.Value = int.Parse(str);
                    break;
                case ParamType.@uint:
                    param.Value = uint.Parse(str);
                    break;
                case ParamType.@float:
                    param.Value = float.Parse(str);
                    break;
                case ParamType.hash40:
                    param.Value = Hash40Util.LabelToHash40(str, stringToHash);
                    break;
                case ParamType.@string:
                    param.Value = str;
                    break;
            }
        }

        /// <summary>
        /// Default case for evaluated commands. Prints the issue message and returns a failed result
        /// </summary>
        static EvalResult Default(string cmd)
        {
            Console.WriteLine($"unknown command {cmd}. Try \'h\' to see available commands");
            return EvalResult.Fail;
        }

        static string ParamInfo(IParam param)
        {
            string type = "(" + param.TypeKey.ToString() + ")";
            switch (param.TypeKey)
            {
                case ParamType.@struct:
                    return type + (param as ParamStruct).Nodes.Count;
                case ParamType.list:
                    return type + (param as ParamList).Nodes.Count;
                default:
                    return type + (param as ParamValue).ToString(hashToString);
            }
        }

        static void PrintCommands()
        {
            //global commands
            Console.WriteLine("Global commands:");
            Console.WriteLine("  h (help) ; s (save) ; so (save overwrite) ; q (quit)");
            Console.WriteLine("  b (go backward in tree) ; pp (print path)");
            //local commands
            IParam local = stack.Peek();
            switch (local.TypeKey)
            {
                case ParamType.@struct:
                    Console.WriteLine("Local commands (struct):");
                    Console.WriteLine("  pc (print children) ; f (forward) [hash or name]");
                    break;
                case ParamType.list:
                    Console.WriteLine("Local commands (array):");
                    Console.WriteLine("  cc (child count) ; f (forward) [index]");
                    break;
                default:
                    Console.WriteLine("Local commands (value):");
                    Console.WriteLine("  = [value]");
                    break;
            }
        }

        enum EvalResult
        {
            Normal,
            Quit,
            Fail
        }
    }
}
