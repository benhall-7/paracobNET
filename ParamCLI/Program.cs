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
            "optional: -h ; -help ; -o [output] ; -l [label file]";
        const string GlobalCommands =
            "global commands (case sensitive):\n" +
            "  -h (help) ; -q (quit) ; -s (save) ; -so (save overwrite)\n" +
            "  -b (go backward in tree) ; -pp (print path)";

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
            labels = new Dictionary<ulong, string>();
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
                                    Console.WriteLine("The file has unsaved changes. Do you want to quit? (n = no, any other = yes)");
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
                            case "-so":
                                stopwatch.Start();
                                paramFile.Save(paramInput);
                                stopwatch.Stop();
                                Console.WriteLine("File saved in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                                break;
                            case "-b":
                                if (stack.Count > 1)
                                    stack.Pop();
                                break;
                            case "-pp":
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
                    EvalCommandForStruct(args);
                    break;
                case ParamType.array:
                    EvalCommandForArray(args);
                    break;
                default:
                    EvalCommandForValue(args);
                    break;
            }
        }

        static void EvalCommandForStruct(string[] args)
        {
            ParamStruct thisParam = stack.Peek() as ParamStruct;
            switch (args[0])
            {
                case "-pc":
                    foreach (var member in thisParam.Nodes)
                        Console.WriteLine(" >" + Hash40Util.FormatToString(member.Key, labels) + ": " + ParamInfo(member.Value));
                    break;
                case "-f":
                    try
                    {
                        ulong value;
                        if (args[1].StartsWith("0x"))
                            value = ulong.Parse(args[1].Substring(2), NumberStyles.HexNumber);
                        else
                            value = Hash40Util.StringToHash40(args[1]);
                        stack.Push(thisParam.Nodes[value]);
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

        static void EvalCommandForArray(string[] args)
        {
            ParamArray paramArray = stack.Peek() as ParamArray;
            switch (args[0])
            {
                case "-cc":
                    Console.WriteLine(paramArray.Nodes.Length);
                    break;
                case "-f":
                    try
                    {
                        stack.Push(paramArray.Nodes[int.Parse(args[1])]);
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

        static void EvalCommandForValue(string[] args)
        {

        }

        static string ParamInfo(IParam param)
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
