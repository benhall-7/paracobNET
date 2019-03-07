using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace paracobNET
{
    public class ParamStruct : ParamBase
    {
        public override ParamType TypeKey { get; } = ParamType.@struct;
        public uint ID { get; set; }
        public Hash40Dictionary<ParamBase> Nodes { get; set; }
        //only used on rebuild
        internal SortedDictionary<ulong, ParamBase> SortedNodes { get; set; }

        public ParamStruct() { }
        public ParamStruct(Hash40Dictionary<ParamBase> nodes)
        {
            Nodes = nodes;
        }

        internal override void Read(BinaryReader reader)
        {
            uint startPos = (uint)reader.BaseStream.Position - 1;
            uint size = reader.ReadUInt32();
            Nodes = new Hash40Dictionary<ParamBase>();

            uint StructRefOffset = reader.ReadUInt32();
            if (ParamFile.StructOffsets.Contains(StructRefOffset))
                ID = (uint)ParamFile.StructOffsets.IndexOf(StructRefOffset);
            else
            {
                ID = (uint)ParamFile.StructOffsets.Count;
                ParamFile.StructOffsets.Add(StructRefOffset);
            }
            reader.BaseStream.Seek(StructRefOffset + ParamFile.RefStart, SeekOrigin.Begin);
            Dictionary<uint, uint> pairs = new Dictionary<uint, uint>();
            for (int i = 0; i < size; i++)
            {
                uint hashIndex = reader.ReadUInt32();
                uint paramOffset = reader.ReadUInt32();
                pairs.Add(hashIndex, paramOffset);
            }
            var hashIndeces = pairs.Keys.ToList();
            hashIndeces.Sort();
            for (int i = 0; i < size; i++)
            {
                var key = hashIndeces[i];
                reader.BaseStream.Seek(startPos + pairs[key], SeekOrigin.Begin);
                ulong hash = ParamFile.DisasmHashTable[key];
                ParamBase param = Util.ReadParam(reader);
                Nodes.Add(hash, param);
            }
        }
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(Nodes.Count);
            writer.Write(ParamFile.RefEntries[(int)ID].offset);

            foreach (var node in SortedNodes)
            {
                if (node.Value.TypeKey == ParamType.@string)
                {
                    var entry = ParamFile.RefEntries[(int)ID];
                    writer.Write((byte)ParamType.@string);
                    writer.Write(entry.stringOffsetPairs[(string)(node.Value as ParamValue).Value] + entry.offset);
                }
                else
                    Util.WriteParam(node.Value, writer);
            }

            SortedNodes = null;
        }
        internal void SetupRefTable()
        {
            SortedNodes = new SortedDictionary<ulong, ParamBase>(Nodes);
            RefTableEntry entry = new RefTableEntry(this);
            int refIndex = ParamFile.RefEntries.IndexOf(entry);
            if (refIndex < 0)
            {
                ID = (uint)ParamFile.RefEntries.Count;
                ParamFile.RefEntries.Add(entry);
            }
            else
            {
                ID = (uint)refIndex;
            }
            entry = ParamFile.RefEntries[(int)ID];

            foreach (var node in SortedNodes)
                Util.ParseParamForRefTables(node.Value, entry);
        }
    }
}
