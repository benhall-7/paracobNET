using paracobNET;
using System.IO;

namespace prcScript
{
    public class LuaParam
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
            return pfile.Root;
        }

        public void save(IParam param, string path)
        {
            if (path == null)
                path = last_dir;
            ParamFile pfile = new ParamFile((ParamStruct)param);
            pfile.Save(path);
        }

        private string fix_path(string rel_path)
        {
            string fix1 = root.Replace('/', '\\');
            string fix2 = rel_path.Replace('/', '\\');
            return Path.Combine(fix1, fix2);
        }
    }
}
