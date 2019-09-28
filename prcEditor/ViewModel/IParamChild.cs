using paracobNET;

namespace prcEditor.ViewModel
{
    /// <summary>
    /// Represents a generic child of a struct
    /// </summary>
    public interface IStructChild
    {
        IParam Param { get; set; }
        ParamType Type { get; set; }
        VM_ParamStruct Parent { get; set; }
        int Index { get; set; }

        string Key { get; set; }
        ulong Hash40 { get; set; }
    }

    /// <summary>
    /// Represents a generic child of a list
    /// </summary>
    public interface IListChild
    {
        IParam Param { get; set; }
        ParamType Type { get; set; }
        VM_ParamList Parent { get; set; }
        int Index { get; set; }
    }
}
