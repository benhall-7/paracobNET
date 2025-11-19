namespace paracobNET;

public static class ParamWriter
{
    public static void Write(ParamContainer container, Stream destination)
    {
        var writer = new Assembler();
        writer.WriteAll(container.Root, destination);
    }
}

internal sealed class Assembler
{
    private readonly List<Hash40> _hashTable = new();
    private readonly List<RefEntry> _refEntries = new();
    private readonly List<(int Offset, ParamMapNode Node)> _unresolvedMaps = new();
    private readonly List<(int Offset, string Value)> _unresolvedStrings = new();
    private readonly Dictionary<ParamMapNode, RefOffsetEntryData> _mapRefDict = new();
    private readonly Dictionary<string, int> _refStringEntries = new();

    public void WriteAll(ParamMapNode root, Stream destination)
    {
        using var headerStream = new MemoryStream();
        using var hashStream = new MemoryStream();
        using var refStream = new MemoryStream();
        using var paramStream = new MemoryStream();

        using var headerWriter = new BinaryWriter(headerStream);
        using var hashWriter = new BinaryWriter(hashStream);
        using var refWriter = new BinaryWriter(refStream);
        using var paramWriter = new BinaryWriter(paramStream);

        for (int i = 0; i < 8; i++)
            headerWriter.Write((byte)ParamContainer.Magic[i]);
        WriteHash(new Hash40(0), hashWriter);
        IterateHashes(root, hashWriter);

        Write(root, paramWriter, hashWriter, refWriter);

        MergeRefTables();
        WriteRefTables(refWriter);
        ResolveStructStringRefs(paramWriter);

        headerWriter.Write((uint)hashWriter.BaseStream.Length);
        headerWriter.Write((uint)refWriter.BaseStream.Length);

        foreach (var writer in new BinaryWriter[] { headerWriter, hashWriter, refWriter, paramWriter })
        {
            writer.BaseStream.Position = 0;
            writer.BaseStream.CopyTo(destination);
        }
    }

    private void Write(ParamNode param, BinaryWriter paramWriter, BinaryWriter hashWriter, BinaryWriter refWriter)
    {
        ParamType type = param.Type;
        paramWriter.Write((byte)type);
        switch (param)
        {
            case ParamMapNode mapNode:
                {
                    var entry = new RefOffsetEntryData(mapNode);
                    _mapRefDict.Add(mapNode, entry);
                    // reserve a space in the file's RefEntries so they stay in order
                    _refEntries.Add(new RefOffsetEntry(entry));

                    var start = paramWriter.BaseStream.Position - 1;
                    paramWriter.Write(mapNode.Entries.Count);

                    _unresolvedMaps.Add(((int)paramWriter.BaseStream.Position, mapNode));
                    paramWriter.Write(0);

                    // ordered by hash index to enable binary search in the binary format
                    foreach (var node in mapNode.Entries.OrderBy(x => x.Key.Value))
                    {
                        int hashIndex = _hashTable.IndexOf(node.Key);
                        int relOffset = (int)(paramWriter.BaseStream.Position - start);
                        entry.HashOffsets.Add(new KeyValuePair<int, int>(hashIndex, relOffset));

                        Write(node.Value, paramWriter, hashWriter, refWriter);
                    }
                    break;
                }
            case ParamArrayNode arrayNode:
                {
                    var startPos = paramWriter.BaseStream.Position - 1;
                    int count = arrayNode.Items.Count;

                    paramWriter.Write(count);

                    int[] offsets = new int[count];
                    long ptrStartPos = paramWriter.BaseStream.Position;
                    paramWriter.BaseStream.Seek(4 * count, SeekOrigin.Current);
                    for (int i = 0; i < count; i++)
                    {
                        var node = arrayNode.Items[i];
                        offsets[i] = (int)(paramWriter.BaseStream.Position - startPos);
                        Write(node, paramWriter, hashWriter, refWriter);
                    }
                    var endPos = paramWriter.BaseStream.Position;
                    paramWriter.BaseStream.Position = ptrStartPos;
                    foreach (var offset in offsets)
                        paramWriter.Write(offset);
                    paramWriter.BaseStream.Position = endPos;

                    break;
                }
            case ParamBoolNode boolNode:
                paramWriter.Write(boolNode.Value);
                break;
            case ParamI8Node i8Node:
                paramWriter.Write(i8Node.Value);
                break;
            case ParamU8Node u8Node:
                paramWriter.Write(u8Node.Value);
                break;
            case ParamI16Node i16Node:
                paramWriter.Write(i16Node.Value);
                break;
            case ParamU16Node u16Node:
                paramWriter.Write(u16Node.Value);
                break;
            case ParamI32Node i32Node:
                paramWriter.Write(i32Node.Value);
                break;
            case ParamU32Node u32Node:
                paramWriter.Write(u32Node.Value);
                break;
            case ParamFloatNode floatNode:
                paramWriter.Write(floatNode.Value);
                break;
            case ParamHash40Node hash40Node:
                paramWriter.Write(_hashTable.IndexOf(hash40Node.Value));
                break;
            case ParamStringNode stringNode:
                string word = stringNode.Value;
                _unresolvedStrings.Add(((int)paramWriter.BaseStream.Position, word));

                AppendRefTableString(word);
                paramWriter.Write(0);
                break;
        }
    }

    private void IterateHashes(ParamNode param, BinaryWriter hashWriter)
    {
        switch (param)
        {
            case ParamMapNode mapNode:
                foreach (var item in mapNode.Entries)
                {
                    WriteHash(item.Key, hashWriter);
                    IterateHashes(item.Value, hashWriter);
                }
                break;
            case ParamArrayNode arrayNode:
                foreach (var item in arrayNode.Items)
                    IterateHashes(item, hashWriter);
                break;
            case ParamHash40Node hash40Node:
                WriteHash(hash40Node.Value, hashWriter);
                break;
        }
    }

    private void WriteHash(Hash40 hash, BinaryWriter hashWriter)
    {
        if (!_hashTable.Contains(hash))
        {
            hashWriter.Write(hash.Value);
            _hashTable.Add(hash);
        }
    }

    private void MergeRefTables()
    {
        var entries = _refEntries;

        for (int index = 0; index < entries.Count; index++)
        {
            switch (entries[index])
            {
                case RefStringEntry:
                    continue;
                case RefOffsetEntry currentTableEntry:
                    // We check if there's aleady an entry prior in the list with the same HashOffsets
                    // if so, we merge
                    int firstIndex = entries.IndexOf(currentTableEntry);
                    if (firstIndex < index)
                    {
                        var first = entries[firstIndex] as RefOffsetEntry;
                        // change the corresponding struct reference
                        _mapRefDict[currentTableEntry.Entry.CorrespondingMapNode] = first.Entry;
                        // remove the duplicate from the list
                        entries.RemoveAt(index--);
                    }
                    break;
            }
        }
    }

    private void AppendRefTableString(string word)
    {
        var entry = new RefStringEntry(word);
        if (!_refEntries.Contains(entry))
            _refEntries.Add(entry);
    }

    private void WriteRefTables(BinaryWriter refWriter)
    {
        foreach (var entry in _refEntries)
        {
            switch (entry)
            {
                case RefOffsetEntry offsetEntry:
                    var entryData = offsetEntry.Entry;
                    entryData.RefTableOffset = (int)refWriter.BaseStream.Position;
                    foreach (var pair in entryData.HashOffsets)
                    {
                        refWriter.Write(pair.Key);
                        refWriter.Write(pair.Value);
                    }
                    break;
                case RefStringEntry stringEntry:
                    var word = stringEntry.Value;
                    _refStringEntries.Add(word, (int)refWriter.BaseStream.Position);
                    for (int c = 0; c < word.Length; c++)
                        refWriter.Write((byte)word[c]);
                    refWriter.Write((byte)0);
                    break;
            }
        }
    }

    private void ResolveStructStringRefs(BinaryWriter paramWriter)
    {
        foreach (var tup in _unresolvedMaps)
        {
            paramWriter.BaseStream.Seek(tup.Item1, SeekOrigin.Begin);
            paramWriter.Write(_mapRefDict[tup.Item2].RefTableOffset);
        }
        foreach (var tup in _unresolvedStrings)
        {
            paramWriter.BaseStream.Seek(tup.Item1, SeekOrigin.Begin);
            paramWriter.Write(_refStringEntries[tup.Item2]);
        }
    }
}
