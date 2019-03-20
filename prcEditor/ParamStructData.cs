using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using paracobNET;

namespace prcEditor
{
    public class ParamStructData
    {
        private ParamStruct UnderlyingStruct { get; set; }

        public ParamStructData(ParamStruct param)
        {
            foreach (var node in param.Nodes)
            {

            }
        }
    }

    public class ParamStructEntry
    {
        private ulong Key { get; set; }
        private IParam Param { get; set; }
        public string Hash
        {
            get { return Hash40Util.FormatToString(Key, MainWindow.HashToStringLabels); }
            set { Key = Hash40Util.LabelToHash40(value, MainWindow.StringToHashLabels); }
        }
        public ParamType Type
        {
            get { return Param.TypeKey; }
            //set { Param.TypeKey = value; }
        }
    }
}
