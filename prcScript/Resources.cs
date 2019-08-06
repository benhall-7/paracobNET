using System.IO;
using System.Reflection;

namespace prcScript
{
    internal static class Resources
    {
        public static string Help { get; private set; }
        public static string LuaAPI { get; private set; }
        public static string LuaSandbox { get; private set; }
        public static string LuaNewTable { get; private set; }

        private static Assembly assembly { get; set; }

        public static void SetUp()
        {
            assembly = typeof(Resources).Assembly;

            Help = GetString("Help.txt");
            LuaAPI = GetString("LuaAPI.txt");
            LuaSandbox = GetString("sandbox.lua");
            LuaNewTable = "return {}";
        }

        private static string GetString(string resPath)
        {
            resPath = nameof(prcScript) + '.' + resPath;
            using (var stream = assembly.GetManifestResourceStream(resPath))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }
    }
}
