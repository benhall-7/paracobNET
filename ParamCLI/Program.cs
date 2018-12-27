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
            "Param Line Interface: Edit param files on the command line\n" +
            "required: [input file]\n" +
            "optional: -h ; -help ; -o [output] ; -l [label file]";
        const string GlobalCommands =
            "global commands (case sensitive):\n" +
            "  -h (help) ; -q (quit) ; -s (save) ; -ow (overwrite save)\n" +
            "  -b (go backward in tree)";

        static ParamFile paramFile { get; set; }
        static string paramInput { get; set; }
        static string paramOutput { get; set; }
        static Dictionary<ulong, string> labels { get; set; }
        static Stopwatch stopwatch { get; set; }

        static Stack<IParam> stack { get; set; }
        static bool edited { get; set; } = false;

        static void Main(string[] args)
        {
            stopwatch = new Stopwatch();
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
                            labels = LabelIO.GetHashStringDict(args[++i]);
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

                EvalGlobal();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                if (!helpPrinted)
                    Console.WriteLine(HelpText);
            }
        }

        static void EvalGlobal()
        {
            Console.WriteLine(GlobalCommands);
            while (true)
            {
                string[] commands = Console.ReadLine().Split(';');
                for (int i = 0; i < commands.Length; i++)
                {
                    stopwatch.Reset();
                    string[] args = commands[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length > 0)
                    {
                        switch (args[0])
                        {
                            case "-h":
                                Console.WriteLine(GlobalCommands);
                                //local commands
                                break;
                            case "-q":
                                if (edited)
                                {
                                    Console.WriteLine("The file has unsaved changes. Are you sure you want to quit? (n = no, any other = yes)");
                                    if (Console.ReadKey().KeyChar == 'n')
                                        break;
                                }
                                return;
                            case "-s":
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
                            case "-ow":
                                stopwatch.Start();
                                paramFile.Save(paramInput);
                                stopwatch.Stop();
                                Console.WriteLine("File saved in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                                break;
                            case "-b":
                                if (stack.Count > 1)
                                    stack.Pop();
                                break;
                            default:
                                EvalCurrentParam(args);
                                break;
                        }
                    }
                }
            }
        }

        static void EvalCurrentParam(string[] args)
        {
            IParam current = stack.Peek();
            switch (current.TypeKey)
            {
                case ParamType.@struct:
                    EvalStruct(args);
                    break;
                case ParamType.array:
                    EvalArray(args);
                    break;
                default:
                    EvalValue(args);
                    break;
            }
        }

        static void EvalStruct(string[] args)
        {
            ParamStruct thisParam = stack.Peek() as ParamStruct;
            switch (args[0])
            {
                case "-pt":
                    foreach (var member in thisParam.Nodes)
                        Console.WriteLine(">" + member.Key.ToString(labels) + ": " + paramInfo(member.Value));
                    break;
                case "-f":
                    try
                    {
                        ulong value;
                        if (args[1].StartsWith("0x"))
                            value = ulong.Parse(args[1].Substring(2), NumberStyles.HexNumber);
                        else
                            value = ulong.Parse(args[1]);
                        stack.Push(thisParam.Nodes[new Hash40(value)]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                default:
                    Console.WriteLine("unknown command {0}", args[0]);
                    break;
            }
        }

        static void EvalArray(string[] args)
        {

        }

        static void EvalValue(string[] args)
        {

        }

        static string paramInfo(IParam param)
        {
            string type = "(" + param.TypeKey.ToString() + ")";
            switch (param.TypeKey)
            {
                case ParamType.@struct:
                    return type + (param as ParamStruct).Nodes.Count;
                case ParamType.array:
                    return type + (param as ParamArray).Nodes.Length;
                default:
                    return type + (param as ParamValue).ToString(labels);
            }
        }
    }
}
