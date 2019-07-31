using NLua;
using paracobNET;
using System;
using System.IO;

namespace prcScript
{
    public class LuaParamGlobal
    {
        //NLua doesn't seem to handle static properties
        //so use the "root" property in lua files
        public static string Root = "";
        public string root
        {
            get => Root;
            set => Root = value;
        }

        public string last_dir { get; set; } = "";

        /// <summary>
        /// Opens a param file given by the path and static Root field
        /// </summary>
        /// <param name="path">Relative or absolute path of file to open</param>
        /// <returns>LuaParam object corresponding to param file's root struct</returns>
        public IParam open(string path)
        {
            last_dir = path;

            ParamFile pfile = new ParamFile();
            pfile.Open(fix_path(path));
            return new LuaParam(pfile.Root);
        }

        public void save(LuaParam param)
        {
            save(param, last_dir);
        }
        public void save(LuaParam param, string path)
        {
            var conversion = (ParamStruct)(IParam)param;
            ParamFile pfile = new ParamFile(conversion);
            pfile.Save(fix_path(path));
        }

        private string fix_path(string rel_path)
        {
            string fix1 = root.Replace('/', '\\');
            string fix2 = rel_path.Replace('/', '\\');
            return Path.Combine(fix1, fix2);
        }
    }
}
