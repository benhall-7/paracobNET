using NLua;
using paracobNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace prcScript
{
    public class Program
    {
        static List<(string, Range)> LuaFiles { get; set; }
        static bool Sandbox { get; set; } = false;
        static bool PrintedHelp { get; set; } = false;

        internal static OrderedDictionary<ulong, string> HashToStringLabels { get; set; }
        internal static OrderedDictionary<string, ulong> StringToHashLabels { get; set; }
        internal static bool LabelsLoaded { get; set; } = false;

        internal static Lua L { get; set; }

        static void Main(string[] args)
        {
            LuaFiles = new List<(string, Range)>();
            HashToStringLabels = new OrderedDictionary<ulong, string>();
            StringToHashLabels = new OrderedDictionary<string, ulong>();
            Resources.SetUp();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Resources.Help);
                        PrintedHelp = true;
                        break;
                    case "-a":
                    case "--api":
                        Console.WriteLine(Resources.LuaAPI);
                        PrintedHelp = true;
                        break;
                    case "-e":
                    case "--env":
                        var last_index = LuaFiles.Count - 1;
                        if (LuaFiles.Count < 0)
                            throw new Exception($"To apply environmental args, lua file must be listed first");
                        var count = int.Parse(args[++i]);
                        LuaFiles[last_index] = (LuaFiles[last_index].Item1, (i + 1)..(i + 1 + count));
                        i += count;
                        break;
                    case "-l":
                    case "--label":
                        HashToStringLabels = LabelIO.GetHashStringDict(args[++i]);
                        StringToHashLabels = LabelIO.GetStringHashDict(args[i]);
                        LabelsLoaded = true;
                        break;
                    case "-s":
                    case "--sandbox":
                        Sandbox = true;
                        break;
                    default:
                        if (args[i].StartsWith("-"))
                            throw new Exception($"unknown command {args[i]}");
                        else
                            LuaFiles.Add((args[i], 0..0));
                        break;
                }
            }

            if (LuaFiles.Count == 0)
            {
                if (!PrintedHelp)
                    Console.WriteLine("No input files specified. See -h for help");
                return;
            }

            foreach (var file_args in LuaFiles)
            {
                using (L = new Lua())
                {
                    L.State.Encoding = Encoding.UTF8;

                    //set globals
                    L["Lib"] = new LuaParamGlobal();
                    L["sandboxed"] = Sandbox;
                    L["labeled"] = LabelsLoaded;
                    L["hash"] = new Func<string, ulong>(Hash40Util.StringToHash40);
                    L["label"] = new Func<ulong, string>((hash) => Hash40Util.FormatToString(hash, HashToStringLabels));
                    L["label2hash"] = new Func<string, ulong>((label) => Hash40Util.LabelToHash40(label, StringToHashLabels));

                    // Environment args passed to lua
                    var range = file_args.Item2;
                    var range_size = range.End.Value - range.Start.Value;
                    if (range_size > 0)
                    {
                        var l_args = Resources.NewTable();

                        for (int i = 1; i <= range_size; i++)
                        {
                            l_args[i] = args[range.Start.Value + i - 1];
                        }

                        L["arg"] = l_args;
                    }

                    L.DoString(Resources.LuaSandbox);

                    L.DoFile(file_args.Item1);
                }
            }
        }
    }
}
