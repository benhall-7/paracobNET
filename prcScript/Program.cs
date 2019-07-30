using NLua;
using paracobNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace prcScript
{
    class Program
    {
        static List<string> LuaFiles { get; set; }
        static bool Sandbox { get; set; } = false;

        static OrderedDictionary<ulong, string> HashToStringLabels { get; set; }

        static Lua L { get; set; }

        static void Main(string[] args)
        {
            LuaFiles = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-s":
                    case "-safe":
                    case "-sandbox":
                        Sandbox = true;
                        break;
                    default:
                        LuaFiles.Add(args[i]);
                        break;
                }
            }

            if (LuaFiles.Count == 0)
                throw new ArgumentException("No files specified. See -h for help");

            foreach (var file in LuaFiles)
            {
                using (L = new Lua())
                {
                    L = new Lua();
                    L.State.Encoding = Encoding.UTF8;
                    //set globals...
                    
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
