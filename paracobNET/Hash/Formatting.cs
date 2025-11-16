namespace paracobNET.Hash40FormattingExtensions;

public static class Hash40FormattingExtensions
{
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

    // TODO: is this really necessary?
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
}