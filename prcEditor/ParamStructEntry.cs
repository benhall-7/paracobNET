using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using paracobNET;

namespace prcEditor
{
    public class ParamStructEntry
    {
        private ulong Key { get; set; }
        private ParamValue Param { get; set; }

        public string Hash
        {
            get { return Hash40Util.FormatToString(Key, MainWindow.HashToStringLabels); }
        }
        public ParamType Type
        {
            get { return Param.TypeKey; }
        }
        public string Value
        {
            get { return Param.ToString(MainWindow.HashToStringLabels); }
            set { Param.SetValue(value, MainWindow.StringToHashLabels); }
        }

        public ParamStructEntry(ulong key, ParamValue param)
        {
            Key = key;
            Param = param;
        }
    }
}
