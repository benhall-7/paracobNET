using paracobNET;

namespace prcEditor
{
    public class ParamStructEntry
    {
        private ulong Hash { get; set; }
        private ParamValue Param { get; set; }

        public string Key
        {
            get { return Hash40Util.FormatToString(Hash, MainWindow.HashToStringLabels); }
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
            Hash = key;
            Param = param;
        }
    }
}
