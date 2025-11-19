namespace paracobNET;

public enum ParamType : byte
{
    Bool = 1,
    I8,
    U8,
    I16,
    U16,
    I32,
    U32,
    Float,
    Hash40,
    String,
    Array,
    Map
}

public static class ParamTypeExtensions
{
    public static string ToStandardName(this ParamType type) => type switch
    {
        ParamType.Bool => "bool",
        ParamType.I8 => "sbyte",
        ParamType.U8 => "byte",
        ParamType.I16 => "short",
        ParamType.U16 => "ushort",
        ParamType.I32 => "int",
        ParamType.U32 => "uint",
        ParamType.Float => "float",
        ParamType.Hash40 => "hash40",
        ParamType.String => "string",
        ParamType.Array => "list",
        ParamType.Map => "struct",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static ParamType FromStandardName(string str) => str switch
    {
        "bool" => ParamType.Bool,
        "sbyte" => ParamType.I8,
        "byte" => ParamType.U8,
        "short" => ParamType.I16,
        "ushort" => ParamType.U16,
        "int" => ParamType.I32,
        "uint" => ParamType.U32,
        "float" => ParamType.Float,
        "hash40" => ParamType.Hash40,
        "string" => ParamType.String,
        "list" => ParamType.Array,
        "struct" => ParamType.Map,
        _ => throw new ArgumentOutOfRangeException(nameof(str), str, null)
    };
}
