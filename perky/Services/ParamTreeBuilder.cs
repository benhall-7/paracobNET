using paracobNET;
using paracobNET.Hash40FormattingExtensions;
using perky.ViewModels;

namespace perky.Services;

public static class ParamTreeBuilder
{
    public static ParamTreeNodeViewModel BuildContainer(ParamContainer container, Labels labels)
    {
        var accessor = new NoAccessor();
        return BuildNode(container.Root, accessor, labels);
    }

    private static ParamTreeNodeViewModel BuildNode(ParamNode node, Accessor accessor, Labels labels)
    {
        var vm = new ParamTreeNodeViewModel(node, accessor, labels);

        switch (node)
        {
            case ParamArrayNode arrayNode:
                {
                    for (int i = 0; i < arrayNode.Count; i++)
                    {
                        var child = arrayNode[i];
                        var indexAccessor = new IndexAccessor(i);
                        vm.Children.Add(BuildNode(child, indexAccessor, labels));
                    }
                    break;
                }
            case ParamMapNode mapNode:
                {
                    foreach (var entry in mapNode.Entries)
                    {
                        // TODO: Use Hash40 formatting extension to get string representation
                        var hash40Accessor = new Hash40Accessor(entry.Key);
                        vm.Children.Add(BuildNode(entry.Value, hash40Accessor, labels));
                    }
                    break;
                }
            default:
                break;
        }

        return vm;
    }
}
