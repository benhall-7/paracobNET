using paracobNET;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

    //param with no parents (must be a struct)
    public class VM_ParamRoot : VM_ParamStruct
    {
        public override string Name => "Root";

        public VM_ParamRoot(ParamStruct struc) : base(struc) { }
    }
}
