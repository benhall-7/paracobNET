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
        public void open(string path, out LuaParam file, out string err)
        {
            err = null;
            file = null;
            last_dir = path;

            ParamFile pfile = new ParamFile();
            string fix1 = OpenDir.Replace('/', '\\');
            string fix2 = path.Replace('/', '\\');
            string real = Path.Combine(fix1, fix2);

            try { pfile.Open(real); }
            catch
            {
                err = $"Unable to open the file '{real}'";
                return;
            }

            file = new LuaParam(pfile.Root);
        }

        public void save(LuaParam param, out bool result, out string err)
        {
            save(param, last_dir, out result, out err);
        }
        public void save(LuaParam param, string path, out bool result, out string err)
        {
            result = false;
            err = null;

            ParamStruct conversion;
            try { conversion = (ParamStruct)param.Inner; }
            catch
            {
                err = $"'save' requires a ParamStruct. Received: {param?.Inner.TypeKey}";
                return;
            }
            ParamFile pfile = new ParamFile(conversion);
            string fix1 = SaveDir.Replace('/', '\\');
            string fix2 = path.Replace('/', '\\');
            string real = Path.Combine(fix1, fix2);

            try { Directory.CreateDirectory(Path.GetDirectoryName(real)); }
            catch (Exception e)
            {
                err = e.Message;
                return;
            }
            try { pfile.Save(real); }
            catch (Exception e)
            {
                err = e.Message;
                return;
            }

            result = true;
        }

        public static void table2param(LuaTable table, out LuaParam param, out string err)
        {
            param = null;
            err = null;

            ParamType ptype;
            try
            {
                var type = (string)table["type"];
                ptype = (ParamType)Enum.Parse(typeof(ParamType), type);
            }
            catch
            {
                err = "Table is missing a 'type' field, or the 'type' field is not a valid paramType";
                return;
            }
            
            switch (ptype)
            {
                case ParamType.@struct:
                    {
                        var s = new ParamStruct();
                        int lua_i = 1;
                        while (table[lua_i++] is LuaTable t)
                        {
                            ulong key;
                            IParam value;
                            try
                            {
                                key = (ulong)t["hash"];
                                value = ((LuaParam)t["param"]).Inner;
                            }
                            catch
                            {
                                err = "Inner table for ParamStruct must have 'hash' (number) and 'param' (LuaParam userdata) fields";
                                return;
                            }
                            s.Nodes.Add(key, value);
                        }
                        param = new LuaParam(s);
                    }
                    break;
                case ParamType.list:
                    {
                        var l = new ParamList();
                        int lua_i = 1;
                        while (table[lua_i++] is LuaParam p)
                            l.Nodes.Add(p.Inner);
                        param = new LuaParam(l);
                    }
                    break;
                default:
                    {
                        var v = new ParamValue(ptype);
                        var value = table["value"].ToString();
                        try
                        {
                            v.SetValue(value, Program.StringToHashLabels);
                        }
                        catch
                        {
                            err = $"Unable to set param value using given value '{value}'";
                            return;
                        }

                        param = new LuaParam(v);
                    }
                    break;
            }
        }
    }
}
