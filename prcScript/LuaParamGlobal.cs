using NLua;
using paracobNET;
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
            pfile.Save(real);
        }

        public LuaParam table2param(LuaTable table)
        {

        }
    }
}
