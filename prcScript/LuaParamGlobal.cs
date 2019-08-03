using NLua;
using paracobNET;
using System;
using System.IO;

namespace prcScript
{
    public class LuaParamGlobal
    {
        //only public, non-static properties exposed to lua context
        internal static string OpenDir { get; set; } = "";
        public string open_dir
        {
            get => OpenDir;
            set => OpenDir = value;
        }

        internal static string SaveDir { get; set; } = "";
        public string save_dir
        {
            get => SaveDir;
            set => SaveDir = value;
        }

        private string last_dir { get; set; } = "";

        /// <summary>
        /// Opens a param file combining path and the static property 'open_dir'
        /// </summary>
        /// <param name="path">Relative or absolute path of file to open</param>
        /// <returns>LuaParam object corresponding to param file's root struct</returns>
        public LuaParam open(string path)
        {
            last_dir = path;

            ParamFile pfile = new ParamFile();
            string fix1 = OpenDir.Replace('/', '\\');
            string fix2 = path.Replace('/', '\\');
            string real = Path.Combine(fix1, fix2);
            pfile.Open(real);
            return new LuaParam(pfile.Root);
        }

        public void save(LuaParam param)
        {
            save(param, last_dir);
        }
        public void save(LuaParam param, string path)
        {
            var conversion = (ParamStruct)param.Inner;
            ParamFile pfile = new ParamFile(conversion);
            string fix1 = SaveDir.Replace('/', '\\');
            string fix2 = path.Replace('/', '\\');
            string real = Path.Combine(fix1, fix2);
            Directory.CreateDirectory(Path.GetDirectoryName(real));
            pfile.Save(real);
        }

        public static LuaParam table2param(LuaTable table)
        {
            string type = (string)table["type"];
            ParamType ptype = (ParamType)Enum.Parse(typeof(ParamType), type);
            switch (ptype)
            {
                case ParamType.@struct:
                    {
                        var s = new ParamStruct();
                        int lua_i = 1;
                        while (table[lua_i++] is LuaTable t)
                        {
                            var key = (ulong)t["hash"];
                            var value = ((LuaParam)t["param"]).Inner;
                            s.Nodes.Add(key, value);
                        }
                        return new LuaParam(s);
                    }
                case ParamType.list:
                    {
                        var l = new ParamList();
                        int lua_i = 1;
                        while (table[lua_i++] is LuaParam p)
                            l.Nodes.Add(p.Inner);
                        return new LuaParam(l);
                    }
                default:
                    var v = new ParamValue(ptype);
                    v.SetValue(table["value"].ToString(), Program.StringToHashLabels);
                    return new LuaParam(v);
            }
        }
    }
}
