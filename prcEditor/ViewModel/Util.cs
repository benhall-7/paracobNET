using paracobNET;

namespace prcEditor.ViewModel
{
    static class Util
    {
        public static string GetStructChildName(IStructChild sc)
        {
            if (sc.Param is ParamStruct || sc.Param is ParamList)
                return sc.Key;
            return $"{sc.Key} ({sc.Param.TypeKey})";
        }

        public static string GetListChildName(IListChild lc)
        {
            if (lc.Param is ParamStruct || lc.Param is ParamList)
                return lc.Index.ToString();
            return $"{lc.Index} ({lc.Param.TypeKey})";
        }
    }
}
