using NLua;
using paracobNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace prcScript
{
    public class Program
    {
        static List<string> LuaFiles { get; set; }
        static bool Sandbox { get; set; } = false;

        internal static OrderedDictionary<ulong, string> HashToStringLabels { get; set; }
        internal static OrderedDictionary<string, ulong> StringToHashLabels { get; set; }

        static Lua L { get; set; }
        static LuaParamGlobal ParamGlobal { get; set; }

        static void Main(string[] args)
        {
            LuaFiles = new List<string>();
            HashToStringLabels = new OrderedDictionary<ulong, string>();
            StringToHashLabels = new OrderedDictionary<string, ulong>();
            ParamGlobal = new LuaParamGlobal();

            LuaFiles.Add("mods.lua");

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
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
                throw new Exception("No files specified. See -h for help");

            foreach (var file in LuaFiles)
            {
                using (L = new Lua())
                {
                    L = new Lua();
                    L.State.Encoding = Encoding.UTF8;
                    //set globals
                    L["Param"] = ParamGlobal;

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
