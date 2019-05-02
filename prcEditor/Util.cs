using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using paracobNET;

namespace prcEditor
{
    static class Util
    {
        public static string GetStructChildName(IParam param, ulong hash40)
        {
            return $"{param.TypeKey} ({Hash40Util.FormatToString(hash40, MainWindow.HashToStringLabels)})";
        }

        public static string GetListChildName(IParam param, int index)
        {
            return $"{param.TypeKey} ({index})";
        }
    }
}
