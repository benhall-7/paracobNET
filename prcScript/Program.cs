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
        static bool PrintedHelp { get; set; } = false;

        internal static OrderedDictionary<ulong, string> HashToStringLabels { get; set; }
        internal static OrderedDictionary<string, ulong> StringToHashLabels { get; set; }

        static void Main(string[] args)
        {
            //args = new string[] { "mods.lua", "-s", "-l", "ParamLabels.csv" };
            LuaFiles = new List<string>();
            HashToStringLabels = new OrderedDictionary<ulong, string>();
            StringToHashLabels = new OrderedDictionary<string, ulong>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "-help":
                        Console.WriteLine(Properties.Resources.Help);
                        PrintedHelp = true;
                        break;
                    case "-a":
                    case "-api":
                        Console.WriteLine(Properties.Resources.LuaAPI);
                        PrintedHelp = true;
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
                if (!PrintedHelp)
                    Console.WriteLine("No input files specified. See -h for help");
                return;
            }

            foreach (var file in LuaFiles)
            {
                using (var L = new Lua())
                {
                    L.State.Encoding = Encoding.UTF8;
                    //set globals
                    L["Param"] = new LuaParamGlobal();
                    L["sandbox"] = Sandbox;

                    L.DoString(Properties.Resources.Sandbox);

                    L.DoFile(file);
                }
            }
        }
    }
}
