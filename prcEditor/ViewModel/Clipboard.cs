using paracobNET;
using System;

namespace prcEditor.ViewModel
{
    [Serializable]
    public class SerializableParam
    {
        public IParam Param { get; set; }

        public SerializableParam(IParam param)
        {
            Param = param;
        }
    }

    [Serializable]
    public class SerializableStructChild : SerializableParam
    {
        public ulong Hash40 { get; set; }

        public SerializableStructChild(IParam param, ulong hash40) : base(param)
        {
            Hash40 = hash40;
        }
    }
}
