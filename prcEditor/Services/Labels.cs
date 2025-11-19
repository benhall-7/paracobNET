using System;
using paracobNET;
using paracobNET.Hash40FormattingExtensions;
using prcEditor.ViewModels;

namespace prcEditor.Services;

public class Labels
{
    public IOrderedDictionary<Hash40, string> HashToStringLabels { get; private set; }
    public IOrderedDictionary<string, Hash40> StringToHashLabels { get; private set; }

    public event Action? LabelsChanged;

    public Labels()
    {
        HashToStringLabels = new paracobNET.OrderedDictionary<Hash40, string>();
        StringToHashLabels = new paracobNET.OrderedDictionary<string, Hash40>();
    }

    public void LoadFromFile(string filepath)
    {
        HashToStringLabels = LabelIO.GetHashStringDict(filepath);
        StringToHashLabels = LabelIO.GetStringHashDict(filepath);
        LabelsChanged?.Invoke();
    }

    public string GetLabel(Accessor accessor)
    {
        return accessor switch
        {
            Hash40Accessor hash40Accessor => hash40Accessor.Value.ToLabelOrHex(HashToStringLabels),
            IndexAccessor indexAccessor => indexAccessor.Value.ToString(),
            NoAccessor => "[Container]",
            _ => throw new ArgumentException("Unknown accessor type")
        };
    }
}