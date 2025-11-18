using paracobNET;
using paracobNET.Hash40FormattingExtensions;
using perky.ViewModels;

namespace perky.Services;

public static class ParamTreeBuilder
{
    public static ParamTreeNodeViewModel BuildContainer(ParamContainer container, string? rootName = null)
    {
        var name = rootName ?? "(Container)";
        return BuildNode(container.Root, name);
    }

    private static ParamTreeNodeViewModel BuildNode(ParamNode node, string displayName)
    {
        var vm = new ParamTreeNodeViewModel(node, displayName);

        switch (node)
        {
            case ParamArrayNode arrayNode:
                {
                    for (int i = 0; i < arrayNode.Count; i++)
                    {
                        var child = arrayNode[i];
                        var childName = $"[{i}]";
                        vm.Children.Add(BuildNode(child, childName));
                    }
                    break;
                }
            case ParamMapNode mapNode:
                {
                    foreach (var entry in mapNode.Entries)
                    {
                        // TODO: Use Hash40 formatting extension to get string representation
                        var keyLabel = entry.Key.ToString();
                        vm.Children.Add(BuildNode(entry.Value, keyLabel));
                    }
                    break;
                }
            default:
                break;
        }

        return vm;
    }
}
