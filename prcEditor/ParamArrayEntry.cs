using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using paracobNET;

namespace prcEditor
{
    public class ParamArrayEntry
    {
        private int Index;
        private ParamValue Param;

        public int Key
        {
            get { return Index; }
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

        public ParamArrayEntry(int key, ParamValue param)
        {
            Index = key;
            Param = param;
        }
    }
}
