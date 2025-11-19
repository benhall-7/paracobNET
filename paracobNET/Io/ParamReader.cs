namespace paracobNET;

public static class ParamReader
{
    public static ParamContainer Read(Stream source)
    {
        var disassembler = new Disassembler();
        var root = disassembler.Start(source);
        return new ParamContainer(root);
    }
}

internal class Disassembler
{
    Hash40[]? HashTable { get; set; }
    Dictionary<int, IOrderedEnumerable<KeyValuePair<int, int>>> RefEntries = new();

    int HashTableSize { get; set; }
    int RefTableSize { get; set; }
    int HashStart => 0x10;
    int RefStart => 0x10 + HashTableSize;
    int ParamStart => 0x10 + HashTableSize + RefTableSize;

    public ParamMapNode Start(Stream source)
    {
        using var Reader = new BinaryReader(source);

        if (Reader.BaseStream.Length < ParamContainer.Magic.Length)
            throw new InvalidDataException("Stream size is too small");
        if (!IsValidMagic(Reader))
        {
            //TODO: use events instead of exceptions to allow
            //user app to determine what to do in weird cases
            //(e.g. user manually replaces "paracobn" header)
            throw new InvalidHeaderException("Stream contains an invalid header. Ensure that the file is a valid param file and that it is not compressed.");
        }

        HashTableSize = Reader.ReadInt32();
        RefTableSize = Reader.ReadInt32();
        HashTable = new Hash40[HashTableSize / 8];

        SetHashTable(Reader);

        Reader.BaseStream.Seek(ParamStart, SeekOrigin.Begin);
        if ((ParamType)Reader.ReadByte() == ParamType.Map)
        {
            Reader.BaseStream.Position -= 1;
            return Read(Reader) as ParamMapNode;
        }
        else
            throw new InvalidDataException("Stream does not have a root param");

    }

    private ParamNode Read(BinaryReader Reader)
    {
        byte key = Reader.ReadByte();
        if (!Enum.IsDefined(typeof(ParamType), key))
            throw new NotImplementedException($"Unimplemented param type '{key}' at {Reader.BaseStream.Position - 1}");
        ParamType type = (ParamType)key;

        ParamNode param;
        switch (type)
        {
            case ParamType.Map:
                {
                    int startPos = (int)Reader.BaseStream.Position - 1;
                    int size = Reader.ReadInt32();
                    int mapRefOffset = Reader.ReadInt32();

                    var map = new ParamMapNode(size);

                    IOrderedEnumerable<KeyValuePair<int, int>> hashOffsets;
                    if (RefEntries.TryGetValue(mapRefOffset, out var refEntry))
                        hashOffsets = refEntry;
                    else
                    {
                        Reader.BaseStream.Position = mapRefOffset + RefStart;
                        var tempHashOffsets = new List<KeyValuePair<int, int>>(size);
                        for (int i = 0; i < size; i++)
                        {
                            int hashIndex = Reader.ReadInt32();
                            int paramOffset = Reader.ReadInt32();
                            tempHashOffsets.Add(new KeyValuePair<int, int>(hashIndex, paramOffset));
                        }
                        hashOffsets = tempHashOffsets.OrderBy(a => a.Key);
                        RefEntries.Add(mapRefOffset, hashOffsets);
                    }

                    foreach (var pair in hashOffsets)
                    {
                        Reader.BaseStream.Position = startPos + pair.Value;
                        Hash40 hash = HashTable[pair.Key];
                        ParamNode child = Read(Reader);
                        map.Add(hash, child);
                    }

                    param = map;
                    break;
                }
            case ParamType.Array:
                {
                    int startPos = (int)Reader.BaseStream.Position - 1;
                    int count = Reader.ReadInt32();
                    uint[] offsets = new uint[count];

                    var list = new ParamArrayNode(count);

                    //all elements should be the same type but it's not enforced

                    for (int i = 0; i < offsets.Length; i++)
                        offsets[i] = Reader.ReadUInt32();

                    for (int i = 0; i < count; i++)
                    {
                        Reader.BaseStream.Position = startPos + offsets[i];
                        list.Add(Read(Reader));
                    }

                    param = list;
                    break;
                }
            case ParamType.Bool:
                var boolValue = Reader.ReadByte() != 0;
                param = new ParamBoolNode(boolValue);
                break;
            case ParamType.I8:
                var i8 = Reader.ReadSByte();
                param = new ParamI8Node(i8);
                break;
            case ParamType.U8:
                var u8 = Reader.ReadByte();
                param = new ParamU8Node(u8);
                break;
            case ParamType.I16:
                var i16 = Reader.ReadInt16();
                param = new ParamI16Node(i16);
                break;
            case ParamType.U16:
                var u16 = Reader.ReadUInt16();
                param = new ParamU16Node(u16);
                break;
            case ParamType.I32:
                var i32 = Reader.ReadInt32();
                param = new ParamI32Node(i32);
                break;
            case ParamType.U32:
                var u32 = Reader.ReadUInt32();
                param = new ParamU32Node(u32);
                break;
            case ParamType.Float:
                var floatValue = Reader.ReadSingle();
                param = new ParamFloatNode(floatValue);
                break;
            case ParamType.Hash40:
                var hash40 = HashTable[Reader.ReadUInt32()];
                param = new ParamHash40Node(hash40);
                break;
            case ParamType.String:
                {
                    long returnTo = Reader.BaseStream.Position;
                    Reader.BaseStream.Seek(RefStart + Reader.ReadInt32(), SeekOrigin.Begin);
                    string s = ""; char c;
                    while ((c = Reader.ReadChar()) != 0)
                        s += c;
                    Reader.BaseStream.Seek(returnTo, SeekOrigin.Begin);
                    param = new ParamStringNode(s);
                    break;
                }
            default:
                throw new Exception("Unreachable code reached in Disassembler.Read()");
        }
        return param;
    }

    private bool IsValidMagic(BinaryReader Reader)
    {
        int len = ParamContainer.Magic.Length;

        Reader.BaseStream.Position = 0;
        for (int i = 0; i < len; i++)
        {
            if (ParamContainer.Magic[i] != Reader.ReadByte())
                return false;
        }
        return true;
    }

    private void SetHashTable(BinaryReader Reader)
    {
        Reader.BaseStream.Position = HashStart;
        for (int i = 0; i < HashTable.Length; i++)
            HashTable[i] = new Hash40(Reader.ReadUInt64());
    }
}

