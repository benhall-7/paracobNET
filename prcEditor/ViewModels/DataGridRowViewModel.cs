using System.ComponentModel;
using System.Runtime.CompilerServices;
using paracobNET;
using prcEditor.Services;

namespace prcEditor.ViewModels;

public class DataGridRowViewModel : INotifyPropertyChanged
{
    private Labels _labels;
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public Accessor Accessor { get; }
    public ParamNode Node { get; }

    public string AccessorText => _labels.GetLabel(Accessor);
    public string TypeText => Node.Type.ToStandardName();

    public ValueEditorViewModel ValueEditor { get; }

    public DataGridRowViewModel(Accessor accessor, ParamNode node, Labels labels)
    {
        Accessor = accessor;
        Node = node;
        _labels = labels;
        _labels.LabelsChanged += () => OnPropertyChanged(nameof(AccessorText));
        ValueEditor = CreateEditor(node, labels);
    }

    private static ValueEditorViewModel CreateEditor(ParamNode node, Labels labels)
        => node switch
        {
            ParamBoolNode b      => new BoolEditorViewModel(b),
            ParamI8Node i8       => NumericOrStringEditorViewModel.For(
                                        () => i8.Value,
                                        v => i8.Value = v,
                                        ParamType.I8),
            ParamU8Node u8       => NumericOrStringEditorViewModel.For(
                                        () => u8.Value,
                                        v => u8.Value = v,
                                        ParamType.U8),
            ParamI16Node i16     => NumericOrStringEditorViewModel.For(
                                        () => i16.Value,
                                        v => i16.Value = v,
                                        ParamType.I16),
            ParamU16Node u16     => NumericOrStringEditorViewModel.For(
                                        () => u16.Value,
                                        v => u16.Value = v,
                                        ParamType.U16),
            ParamI32Node i32     => NumericOrStringEditorViewModel.For(
                                        () => i32.Value,
                                        v => i32.Value = v,
                                        ParamType.I32),
            ParamU32Node u32     => NumericOrStringEditorViewModel.For(
                                        () => u32.Value,
                                        v => u32.Value = v,
                                        ParamType.U32),
            ParamFloatNode f32   => NumericOrStringEditorViewModel.For(
                                        () => f32.Value,
                                        v => f32.Value = v,
                                        ParamType.Float),
            ParamStringNode str  => NumericOrStringEditorViewModel.For(
                                        () => str.Value,
                                        v => str.Value = v,
                                        ParamType.String),

            ParamHash40Node h40  => new Hash40EditorViewModel(h40, labels),

            ParamArrayNode       => new DisabledEditorViewModel(),
            ParamMapNode         => new DisabledEditorViewModel(),

            _                    => new DisabledEditorViewModel(),
        };
    
    protected void OnLabelsChanged() => OnPropertyChanged(nameof(AccessorText));
}
