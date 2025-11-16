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
}