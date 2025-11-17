namespace paracobNET;

[Serializable]
public sealed class ParamArrayNode : ParamNode
{
    public override ParamType Type => ParamType.Array;

    private readonly List<ParamNode> _items;

    // Expose read-only view to consumers
    public IReadOnlyList<ParamNode> Items => _items;

    public ParamArrayNode()
    {
        _items = new List<ParamNode>();
    }

    public ParamArrayNode(int capacity)
    {
        _items = new List<ParamNode>(capacity);
    }

    public ParamArrayNode(IEnumerable<ParamNode> items)
    {
        _items = [.. items];
    }

    public void Add(ParamNode node) => _items.Add(node);

    public void Insert(int index, ParamNode node) => _items.Insert(index, node);

    public bool Remove(ParamNode node) => _items.Remove(node);

    public void RemoveAt(int index) => _items.RemoveAt(index);

    public ParamNode this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public int Count => _items.Count;

    public override ParamNode Clone()
    {
        var clonedItems = _items.Select(item => (ParamNode)item.Clone());
        return new ParamArrayNode(clonedItems);
    }
}
