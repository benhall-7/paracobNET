namespace paracobNET;

internal class Assembler
{
    ParamMapNode Root { get; set; }
    string Filepath { get; set; }

    FileStream FileStream { get; set; }

    BinaryWriter WriterHeader { get; set; } //header stream
    BinaryWriter WriterHash { get; set; }   //hash table stream
    BinaryWriter WriterRef { get; set; }    //reference table stream
    BinaryWriter WriterParam { get; set; }  //param stream

    List<ulong> AsmHashTable { get; set; }//list of hashes appended to
    List<object> AsmRefEntries { get; set; }//reference entry classes, and strings
    Dictionary<ParamMapNode, RefTableEntry> StructRefEntries { get; set; }

    List<Tuple<int, ParamMapNode>> UnresolvedStructs { get; set; }
    List<Tuple<int, string>> UnresolvedStrings { get; set; }
    Dictionary<string, int> RefStringEntries { get; set; }

    public Assembler(string filepath, ParamMapNode root)
    {
        Root = root;
        Filepath = filepath;

        AsmHashTable = new List<ulong>();
        AsmRefEntries = new List<object>();
        StructRefEntries = new Dictionary<ParamMapNode, RefTableEntry>();
        UnresolvedStructs = new List<Tuple<int, ParamMapNode>>();
        UnresolvedStrings = new List<Tuple<int, string>>();
        RefStringEntries = new Dictionary<string, int>();
    }

    public void Start()
    {
        using (FileStream = File.Create(Filepath))
        using (WriterHeader = new BinaryWriter(new MemoryStream()))
        using (WriterHash = new BinaryWriter(new MemoryStream()))
        using (WriterRef = new BinaryWriter(new MemoryStream()))
        using (WriterParam = new BinaryWriter(new MemoryStream()))
        {
            for (int i = 0; i < 8; i++)
                WriterHeader.Write((byte)ParamFile.Magic[i]);
            WriteHash(0);
            IterateHashes(Root);

            Write(Root);

            MergeRefTables();
            WriteRefTables();
            ResolveStructStringRefs();

            WriterHeader.Write((uint)WriterHash.BaseStream.Length);
            WriterHeader.Write((uint)WriterRef.BaseStream.Length);

            foreach (var writer in new BinaryWriter[] { WriterHeader, WriterHash, WriterRef, WriterParam })
            {
                writer.BaseStream.Position = 0;
                writer.BaseStream.CopyTo(FileStream);
            }
        }
    }

    private void Write(IParam param)
    {
        ParamType type = param.Type;
        WriterParam.Write((byte)type);
        switch (type)
        {
            case ParamType.Map:
                {
                    var str = param as ParamMapNode;
                    var entry = new RefTableEntry(str);
                    StructRefEntries.Add(str, entry);
                    AsmRefEntries.Add(entry);//reserve a space in the file's RefEntries so they stay in order

                    var start = WriterParam.BaseStream.Position - 1;
                    WriterParam.Write(str.Nodes.Count);

                    UnresolvedStructs.Add(new Tuple<int, ParamMapNode>((int)WriterParam.BaseStream.Position, str));
                    WriterParam.Write(0);

                    foreach (var node in str.Nodes.OrderBy(x => x.Key))
                    {
                        int hashIndex = AsmHashTable.IndexOf(node.Key);
                        int relOffset = (int)(WriterParam.BaseStream.Position - start);
                        entry.HashOffsets.Add(new KeyValuePair<int, int>(hashIndex, relOffset));

                        Write(node.Value);
                    }
                    break;
                }
            case ParamType.Array:
                {
                    var list = param as ParamArrayNode;
                    var startPos = WriterParam.BaseStream.Position - 1;
                    int count = list.Entries.Count;

                    WriterParam.Write(count);

                    int[] offsets = new int[count];
                    long ptrStartPos = WriterParam.BaseStream.Position;
                    WriterParam.BaseStream.Seek(4 * count, SeekOrigin.Current);
                    for (int i = 0; i < count; i++)
                    {
                        var node = list.Entries[i];
                        offsets[i] = (int)(WriterParam.BaseStream.Position - startPos);
                        Write(node);
                    }
                    var endPos = WriterParam.BaseStream.Position;
                    WriterParam.BaseStream.Position = ptrStartPos;
                    foreach (var offset in offsets)
                        WriterParam.Write(offset);
                    WriterParam.BaseStream.Position = endPos;

                    break;
                }
            default:
                {
                    object value = (param as ParamValueNode).Value;
                    switch (type)
                    {
                        case ParamType.Bool:
                            WriterParam.Write((bool)value);
                            break;
                        case ParamType.I8:
                            WriterParam.Write((sbyte)value);
                            break;
                        case ParamType.U8:
                            WriterParam.Write((byte)value);
                            break;
                        case ParamType.I16:
                            WriterParam.Write((short)value);
                            break;
                        case ParamType.U16:
                            WriterParam.Write((ushort)value);
                            break;
                        case ParamType.I32:
                            WriterParam.Write((int)value);
                            break;
                        case ParamType.U32:
                            WriterParam.Write((uint)value);
                            break;
                        case ParamType.Float:
                            WriterParam.Write((float)value);
                            break;
                        case ParamType.Hash40:
                            WriterParam.Write(AsmHashTable.IndexOf((ulong)value));
                            break;
                        case ParamType.String:
                            string word = (string)value;
                            UnresolvedStrings.Add(new Tuple<int, string>((int)WriterParam.BaseStream.Position, word));

                            AppendRefTableString(word);
                            WriterParam.Write(0);
                            break;
                    }

                    break;
                }
        }
    }

    private void IterateHashes(IParam param)
    {
        switch (param.Type)
        {
            case ParamType.Map:
                foreach (var item in (param as ParamMapNode).Nodes)
                {
                    WriteHash(item.Key);
                    IterateHashes(item.Value);
                }
                break;
            case ParamType.Array:
                foreach (var item in (param as ParamArrayNode).Entries)
                    IterateHashes(item);
                break;
            case ParamType.Hash40:
                WriteHash((ulong)(param as ParamValueNode).Value);
                break;
        }
    }

    private void WriteHash(ulong hash)
    {
        if (!AsmHashTable.Contains(hash))
        {
            WriterHash.Write(hash);
            AsmHashTable.Add(hash);
        }
    }

    private void MergeRefTables()
    {
        var entries = AsmRefEntries;

        for (int current_index = 0; current_index < entries.Count; current_index++)
        {
            if (!(entries[current_index] is RefTableEntry currentTableEntry))
                continue;

            //We check if there's aleady an entry prior in the list with the same HashOffsets
            //if so, we merge
            int firstOccur = entries.IndexOf(currentTableEntry);
            if (firstOccur < current_index)
            {
                var first = entries[firstOccur] as RefTableEntry;
                //change the corresponding struct reference
                StructRefEntries[currentTableEntry.CorrespondingStruct] = first;
                //remove the duplicate from the list
                entries.RemoveAt(current_index--);
            }
        }
    }

    private void AppendRefTableString(string word)
    {
        if (!AsmRefEntries.Contains(word))
            AsmRefEntries.Add(word);
    }

    private void WriteRefTables()
    {
        var writer = WriterRef;
        foreach (var entry in AsmRefEntries)
        {
            if (entry is RefTableEntry refEntry)
            {
                refEntry.RefTableOffset = (int)writer.BaseStream.Position;
                foreach (var pair in refEntry.HashOffsets)
                {
                    writer.Write(pair.Key);
                    writer.Write(pair.Value);
                }
            }
            else if (entry is string word)
            {
                RefStringEntries.Add(word, (int)writer.BaseStream.Position);
                for (int c = 0; c < word.Length; c++)
                    writer.Write((byte)word[c]);
                writer.Write((byte)0);
            }
        }
    }

    private void ResolveStructStringRefs()
    {
        var writer = WriterParam;
        foreach (var tup in UnresolvedStructs)
        {
            writer.BaseStream.Seek(tup.Item1, SeekOrigin.Begin);
            writer.Write(StructRefEntries[tup.Item2].RefTableOffset);
        }
        foreach (var tup in UnresolvedStrings)
        {
            writer.BaseStream.Seek(tup.Item1, SeekOrigin.Begin);
            writer.Write(RefStringEntries[tup.Item2]);
        }
    }
}
