using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Markup.Xaml.MarkupExtensions;
using paracobNET;
using paracobNET.Hash40FormattingExtensions;
using prcEditor.Services;

namespace prcEditor.ViewModels;

public abstract class ValueEditorViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class BoolEditorViewModel : ValueEditorViewModel
{
    private readonly ParamBoolNode _node;

    public BoolEditorViewModel(ParamBoolNode node)
    {
        _node = node;
    }

    public bool Value
    {
        get => _node.Value;
        set
        {
            if (_node.Value == value) return;
            _node.Value = value;
            OnPropertyChanged();
        }
    }
}

public sealed class NumericOrStringEditorViewModel : ValueEditorViewModel
{
    private readonly Func<object> _get;
    private readonly Action<object> _set;
    private readonly ParamType _paramType;

    private string _text;

    public RelayCommand CommitCommand { get; }

    private NumericOrStringEditorViewModel(
        Func<object> get,
        Action<object> set,
        ParamType paramType)
    {
        _get = get;
        _set = set;
        _paramType = paramType;
        CommitCommand = new RelayCommand(_ => Commit());

        _text = Format(_get());
    }

    public static NumericOrStringEditorViewModel For<T>(
        Func<T> get,
        Action<T> set,
        ParamType paramType)
    {
        return new NumericOrStringEditorViewModel(
            () => get()!,
            v => set((T)v),
            paramType);
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            OnPropertyChanged();
        }
    }

    public void Commit()
    {
        try
        {
            var parsed = Parse(_text, _paramType);
            Console.WriteLine($"Parsed value: {parsed}");
            _set(parsed);
        }
        catch
        {
            // TODO: notify the user of parsing failure
            // parse failure case: revert text to previous state
            _text = Format(_get());
            OnPropertyChanged(nameof(Text));
        }
    }

    private static string Format(object value)
        => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

    private static object Parse(string text, ParamType type)
        => type switch
        {
            ParamType.I8 => sbyte.Parse(text, CultureInfo.InvariantCulture),
            ParamType.U8 => byte.Parse(text, CultureInfo.InvariantCulture),
            ParamType.I16 => short.Parse(text, CultureInfo.InvariantCulture),
            ParamType.U16 => ushort.Parse(text, CultureInfo.InvariantCulture),
            ParamType.I32 => int.Parse(text, CultureInfo.InvariantCulture),
            ParamType.U32 => uint.Parse(text, CultureInfo.InvariantCulture),
            ParamType.Float => float.Parse(text, CultureInfo.InvariantCulture),
            ParamType.String => text,
            _ => text
        };
}

public sealed class Hash40EditorViewModel : ValueEditorViewModel
{
    private readonly ParamHash40Node _node;
    private readonly Labels _labels;
    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            OnPropertyChanged();
        }
    }

    private bool _isDropDownOpen;
    public bool IsDropDownOpen
    {
        get => _isDropDownOpen;
        set
        {
            if (_isDropDownOpen == value) return;
            _isDropDownOpen = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand CommitCommand { get; }

    public IReadOnlyList<string> Suggestions => ((IReadOnlyDictionary<string, Hash40>)_labels.StringToHashLabels).Keys.ToList();

    public Hash40EditorViewModel(ParamHash40Node node, Labels labels)
    {
        _node = node;
        _labels = labels;
        _text = _node.Value.ToLabelOrHex(_labels.HashToStringLabels);
        CommitCommand = new RelayCommand(_ => Commit());

        _labels.LabelsChanged += OnLabelsChanged;
    }

    ~Hash40EditorViewModel()
    {
        _labels.LabelsChanged -= OnLabelsChanged;
    }

    public void Commit()
    {
        try
        {
            _node.Value = Hash40FormattingExtensions.FromLabelOrHex(Text, _labels.StringToHashLabels);
            IsDropDownOpen = false;
        }
        catch
        {
            // TODO: notify the user of parsing failure
            // parse failure case: revert text to previous state
            _text = _node.Value.ToLabelOrHex(_labels.HashToStringLabels);
            OnPropertyChanged(nameof(Text));
        }
    }

    public void OnLabelsChanged()
    {
        _text = _node.Value.ToLabelOrHex(_labels.HashToStringLabels);
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(Suggestions));
    }
}

public sealed class DisabledEditorViewModel : ValueEditorViewModel
{
    public string Display => string.Empty;
}
