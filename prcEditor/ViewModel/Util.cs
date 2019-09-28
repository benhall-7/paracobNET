using paracobNET;
using System;

namespace prcEditor.ViewModel
{
    //TODO: fully deprecated because of Default Interface Implementations.
    //Awaiting update to NET 3.0
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
