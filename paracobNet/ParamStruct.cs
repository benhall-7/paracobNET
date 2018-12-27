using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace paracobNET
{
    public class ParamStruct : IParam
    {
        public ParamType TypeKey { get; } = ParamType.@struct;
        public uint ID { get; set; }
        public Dictionary<Hash40, IParam> Nodes { get; set; }
        //only used on rebuild
        internal SortedDictionary<Hash40, IParam> SortedNodes { get; set; }

        internal void Read()
        {
            var reader = ParamFile.Reader;
            uint startPos = (uint)reader.BaseStream.Position - 1;
            uint size = reader.ReadUInt32();
            Nodes = new Dictionary<Hash40, IParam>();

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
                Hash40 hash = ParamFile.DisasmHashTable[key];
                IParam param = Util.ReadParam();
                Nodes.Add(hash, param);
            }
        }
        internal void SetupRefTable()
        {
            SortedNodes = new SortedDictionary<Hash40, IParam>(Nodes);
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
        internal void Write()
        {
            var paramWriter = ParamFile.WriterParam;
            paramWriter.Write(Nodes.Count);
            paramWriter.Write(ParamFile.RefEntries[(int)ID].offset);

            foreach (var node in SortedNodes)
            {
                if (node.Value.TypeKey == ParamType.@string)
                {
                    var entry = ParamFile.RefEntries[(int)ID];
                    paramWriter.Write(entry.stringOffsetPairs[(string)(node.Value as ParamValue).Value] + entry.offset);
                }
                else
                    Util.WriteParam(node.Value);
            }

            SortedNodes = null;
        }
    }
}
