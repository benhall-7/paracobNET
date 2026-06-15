using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using paracobNET;
using prcEditor.Services;

namespace prcEditor.ViewModels;

public abstract record Accessor;

public sealed record Hash40Accessor(Hash40 Value) : Accessor;
public sealed record IndexAccessor(int Value) : Accessor;
public sealed record NoAccessor() : Accessor;

public class ParamTreeNodeViewModel : INotifyPropertyChanged
{
    private Labels _labels { get; }
    private ParamNode _node;
    public ParamNode Node
    {
        get => _node;
        private set
        {
            if (ReferenceEquals(_node, value)) return;
            _node = value;
            OnPropertyChanged();
        }
    }

    public ParamTreeNodeViewModel? Parent { get; }
    public Accessor Accessor { get; }
    public ObservableCollection<ParamTreeNodeViewModel> Children { get; } = new();

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string DisplayName => _labels.GetLabel(Accessor);

    public ParamTreeNodeViewModel(ParamNode node, ParamTreeNodeViewModel? parent, Accessor accessor, Labels labels)
    {
        _node = node;
        Accessor = accessor;
        Parent = parent;

        _labels = labels;
        _labels.LabelsChanged += OnLabelsChanged;
    }

    ~ParamTreeNodeViewModel()
    {
        _labels.LabelsChanged -= OnLabelsChanged;
    }

    public void ExpandParents()
    {
        Parent?.Expand();
    }

    public void ReplaceNode(ParamNode node)
    {
        Node = node;
        Children.Clear();
        ParamTreeBuilder.BuildChildren(this, node, _labels);
    }

    private void Expand()
    {
        Parent?.Expand();
        IsExpanded = true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected void OnLabelsChanged() => OnPropertyChanged(nameof(DisplayName));
}
