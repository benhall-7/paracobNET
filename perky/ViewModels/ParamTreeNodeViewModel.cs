using System.Collections.ObjectModel;
using paracobNET;

namespace perky.ViewModels;

public class ParamTreeNodeViewModel
{
    public ParamNode Node { get; }
    public string DisplayName { get; }
    public ObservableCollection<ParamTreeNodeViewModel> Children { get; } = new();

    public ParamTreeNodeViewModel(ParamNode node, string displayName)
    {
        Node = node;
        DisplayName = displayName;
    }
}
