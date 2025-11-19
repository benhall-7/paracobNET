namespace paracobNET;

internal abstract record RefEntry;

internal sealed record RefOffsetEntry(RefOffsetEntryData Entry) : RefEntry;

internal sealed record RefStringEntry(string Value) : RefEntry;

internal class RefOffsetEntryData
{
    public int RefTableOffset { get; set; }
    public List<KeyValuePair<int, int>> HashOffsets { get; set; }
    public ParamMapNode CorrespondingMapNode { get; set; }

    public RefOffsetEntryData(ParamMapNode correspondingMapNode)
    {
        CorrespondingMapNode = correspondingMapNode;
        HashOffsets = new List<KeyValuePair<int, int>>();
    }

    public override bool Equals(object other)
    {
        if (!(other is RefOffsetEntryData entry))
            return false;
        if (base.Equals(entry))
            return true;
        //thanks to Nick Jones: https://stackoverflow.com/a/3804852
        if (HashOffsets.Count == entry.HashOffsets.Count && !HashOffsets.Except(entry.HashOffsets).Any())
            return true;
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

