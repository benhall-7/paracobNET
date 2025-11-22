namespace paracobNET.Extensions;

public static class Extensions
{
    /// <summary>
    /// Returns a human-readable string representation of the ParamNode, using labels for Hash40's if provided.
    /// </summary>
    public static string ToPrettyString(
        this ParamNode node,
        IReadOnlyDictionary<Hash40, string>? labels
    )
    {
        var standardName = ParamTypeExtensions.ToStandardName(node.Type);
        switch (node)
        {
            case ParamMapNode:
            case ParamArrayNode:
                return standardName;
            case ParamBoolNode boolNode:
                return $"({standardName}){boolNode.Value}";
            case ParamI8Node i8Node:
                return $"({standardName}){i8Node.Value}";
            case ParamU8Node u8Node:
                return $"({standardName}){u8Node.Value}";
            case ParamI16Node i16Node:
                return $"({standardName}){i16Node.Value}";
            case ParamU16Node u16Node:
                return $"({standardName}){u16Node.Value}";
            case ParamI32Node i32Node:
                return $"({standardName}){i32Node.Value}";
            case ParamU32Node u32Node:
                return $"({standardName}){u32Node.Value}";
            case ParamFloatNode floatNode:
                return $"({standardName}){floatNode.Value}";
            case ParamHash40Node hash40Node:
                return $"({standardName}){hash40Node.Value.ToLabelOrHex(labels)}";
            case ParamStringNode stringNode:
                return $"({standardName})\"{stringNode.Value}\"";
            default:
                return standardName;
        }
    }

    /// <summary>
    /// Returns the label for this Hash40 if present in the map, otherwise the hex representation.
    /// </summary>
    public static string ToLabelOrHex(
        this Hash40 hash,
        IReadOnlyDictionary<Hash40, string>? labels)
    {
        if (labels != null && labels.TryGetValue(hash, out var label))
            return label;

        return hash.ToString();
    }

    /// <summary>
    /// Returns the Hash40 for this string if it is a valid hex value or label, otherwise throws an exception.
    /// </summary>
    public static Hash40 FromLabelOrHex(
        string str,
        IReadOnlyDictionary<string, Hash40>? labels)
    {
        if (str.StartsWith("0x") &&
            ulong.TryParse(str.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out ulong val))
        {
            return new Hash40(val);
        }

        if (labels != null && labels.TryGetValue(str, out var hash))
            return hash;

        throw new InvalidLabelException($"The provided string is not valid hexadecimal and has no matching label", str);
    }

    /// <summary>
    /// Attempts to return the Hash40 for this string if it is a valid hex value or label, otherwise returns null.
    /// </summary>
    public static Hash40? TryFromLabelOrHex(
        string str,
        IReadOnlyDictionary<string, Hash40>? labels)
    {
        if (str.StartsWith("0x") &&
            ulong.TryParse(str.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out ulong val))
        {
            return new Hash40(val);
        }

        if (labels != null && labels.TryGetValue(str, out var hash))
            return hash;

        return null;
    }
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
