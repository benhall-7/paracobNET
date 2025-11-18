using System;
using paracobNET;
using paracobNET.Hash40FormattingExtensions;
using perky.ViewModels;

namespace perky.Services;

public class Labels
{
    public IOrderedDictionary<Hash40, string> hashToStringLabels { get; private set; }
    public IOrderedDictionary<string, Hash40> stringToHashLabels { get; private set; }

    public event Action? LabelsChanged;

    public Labels()
    {
        hashToStringLabels = new paracobNET.OrderedDictionary<Hash40, string>();
        stringToHashLabels = new paracobNET.OrderedDictionary<string, Hash40>();
    }

    public void LoadFromFile(string filepath)
    {
        hashToStringLabels = LabelIO.GetHashStringDict(filepath);
        stringToHashLabels = LabelIO.GetStringHashDict(filepath);
        LabelsChanged?.Invoke();
    }

    public string GetLabel(Accessor accessor)
    {
        return accessor switch
        {
            Hash40Accessor hash40Accessor => hash40Accessor.Value.ToLabelOrHex(hashToStringLabels),
            IndexAccessor indexAccessor => indexAccessor.Value.ToString(),
            NoAccessor => "[Container]",
            _ => throw new ArgumentException("Unknown accessor type")
        };
    }
}