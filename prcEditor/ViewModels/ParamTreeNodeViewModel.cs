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
    public ParamNode Node { get; }
    public Accessor Accessor { get; }
    public ObservableCollection<ParamTreeNodeViewModel> Children { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string DisplayName => _labels.GetLabel(Accessor);

    public ParamTreeNodeViewModel(ParamNode node, Accessor accessor, Labels labels)
    {
        Node = node;
        Accessor = accessor;

        _labels = labels;
        _labels.LabelsChanged += OnLabelsChanged;
    }

    ~ParamTreeNodeViewModel()
    {
        _labels.LabelsChanged -= OnLabelsChanged;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected void OnLabelsChanged() => OnPropertyChanged(nameof(DisplayName));
}
