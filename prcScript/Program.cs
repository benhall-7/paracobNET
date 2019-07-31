using NLua;
using paracobNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace prcScript
{
    public class Program
    {
        static string HelpText = "prcScript: edit params through lua\n" +
            "required: [script file] (allows multiple files)\n" +
            "optional: \n" +
            "  -h = print help text\n" +
            "       (alias: -help)\n" +
            "  -s = sandbox lua environment (prevents running unsafe code)\n" +
            "       (alias: -safe | -sandbox)\n" +
            "  -l = load label file [path]";

        static List<string> LuaFiles { get; set; }
        static bool Sandbox { get; set; } = false;

        internal static OrderedDictionary<ulong, string> HashToStringLabels { get; set; }
        internal static OrderedDictionary<string, ulong> StringToHashLabels { get; set; }

        static Lua L { get; set; }
        static LuaParamGlobal ParamGlobal { get; set; }

        static void Main(string[] args)
        {
            //args = new string[] { "mods.lua", "-s", "-l", "ParamLabels.csv" };
            LuaFiles = new List<string>();
            HashToStringLabels = new OrderedDictionary<ulong, string>();
            StringToHashLabels = new OrderedDictionary<string, ulong>();
            ParamGlobal = new LuaParamGlobal();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "-help":
                        Console.WriteLine(HelpText);
                        break;
                    case "-l":
                        HashToStringLabels = LabelIO.GetHashStringDict(args[++i]);
                        StringToHashLabels = LabelIO.GetStringHashDict(args[i]);
                        break;
                    case "-s":
                    case "-safe":
                    case "-sandbox":
                        Sandbox = true;
                        break;
                    default:
                        if (args[i].StartsWith("-"))
                            throw new Exception($"unknown command {args[i]}");
                        else
                            LuaFiles.Add(args[i]);
                        break;
                }
            }

            if (LuaFiles.Count == 0)
            {
                Console.WriteLine("No input files specified. See -h for help");
                return;
            }

            foreach (var file in LuaFiles)
            {
                using (L = new Lua())
                {
                    L = new Lua();
                    L.State.Encoding = Encoding.UTF8;
                    //set globals
                    L["Param"] = ParamGlobal;
                    //add some timing functions in case the lua environment is sandboxed
                    L.DoString("time = os.time\n" +
                        "clock = os.clock\n" +
                        "date = os.date");

                    if (Sandbox)
                    {
                        L.DoString("debug = nil\n" +
                            "io = nil\n" +
                            "luanet = nil\n" +
                            "os = nil\n" +
                            "package = nil\n" +
                            "collectgarbage = nil\n" +
                            "dofile = nil\n" +
                            "load = nil\n" +
                            "loadfile = nil\n" +
                            "require = nil");
                    }

                    L.DoFile(file);
                }
            }
        }
    }
}
