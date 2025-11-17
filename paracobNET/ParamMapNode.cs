namespace paracobNET;

[Serializable]
public class ParamMapNode : ParamNode
{
    public override ParamType Type => ParamType.Map;

    // Ordered dictionary emulation
    private readonly List<KeyValuePair<Hash40, ParamNode>> _entries;

    public IList<KeyValuePair<Hash40, ParamNode>> Entries => _entries;

    public ParamMapNode(int capacity)
    {
        _entries = new List<KeyValuePair<Hash40, ParamNode>>(capacity);
    }
    public ParamMapNode(IEnumerable<KeyValuePair<Hash40, ParamNode>> entries)
    {
        _entries = [.. entries];
    }

    public ParamNode? Get(Hash40 key) =>
        _entries.FirstOrDefault(kvp => kvp.Key == key).Value;

    public void Add(Hash40 key, ParamNode value) =>
        _entries.Add(new KeyValuePair<Hash40, ParamNode>(key, value));

    public bool Remove(Hash40 key)
    {
        int index = _entries.FindIndex(kvp => kvp.Key == key);
        if (index == -1) return false;
        _entries.RemoveAt(index);
        return true;
    }

    public override object Clone()
    {
        var clonedEntries = _entries.Select(kvp =>
            new KeyValuePair<Hash40, ParamNode>(kvp.Key, (ParamNode)kvp.Value.Clone()));
        return new ParamMapNode(clonedEntries);
    }
}
