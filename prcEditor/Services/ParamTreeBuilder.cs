using paracobNET;
using prcEditor.ViewModels;

namespace prcEditor.Services;

public static class ParamTreeBuilder
{
    public static ParamTreeNodeViewModel BuildContainer(ParamContainer container, Labels labels)
    {
        var accessor = new NoAccessor();
        return BuildNode(container.Root, null, accessor, labels);
    }

    private static ParamTreeNodeViewModel BuildNode(ParamNode node, ParamTreeNodeViewModel? parent, Accessor accessor, Labels labels)
    {
        var vm = new ParamTreeNodeViewModel(node, parent, accessor, labels);
        BuildChildren(vm, node, labels);
        return vm;
    }

    public static void BuildChildren(ParamTreeNodeViewModel parent, ParamNode node, Labels labels)
    {
        switch (node)
        {
            case ParamArrayNode arrayNode:
                {
                    for (int i = 0; i < arrayNode.Count; i++)
                    {
                        var child = arrayNode[i];
                        var indexAccessor = new IndexAccessor(i);
                        parent.Children.Add(BuildNode(child, parent, indexAccessor, labels));
                    }
                    break;
                }
            case ParamMapNode mapNode:
                {
                    foreach (var entry in mapNode.Entries)
                    {
                        var hash40Accessor = new Hash40Accessor(entry.Key);
                        parent.Children.Add(BuildNode(entry.Value, parent, hash40Accessor, labels));
                    }
                    break;
                }
            default:
                break;
        }
    }
}
