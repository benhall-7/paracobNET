using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace paracobNET
{
    public class ParamStruct : IParam
    {
        public ParamType TypeKey { get; } = ParamType.@struct;
        public int ID { get; set; }
        public Hash40Dictionary<IParam> Nodes { get; set; }

        //only used on rebuild
        internal RefTableEntry RefEntry { get; set; }

        public ParamStruct() { }
        public ParamStruct(Hash40Dictionary<IParam> nodes)
        {
            Nodes = nodes;
        }

        internal void Read(BinaryReader reader)
        {
            uint startPos = (uint)reader.BaseStream.Position - 1;
            uint size = reader.ReadUInt32();
            Nodes = new Hash40Dictionary<IParam>();

            uint StructRefOffset = reader.ReadUInt32();
            if (ParamFile.StructOffsets.Contains(StructRefOffset))
                ID = ParamFile.StructOffsets.IndexOf(StructRefOffset);
            else
            {
                ID = ParamFile.StructOffsets.Count;
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
                IParam param = Util.ReadParam(reader);
                Nodes.Add(hash, param);
            }
        }
        internal void Write(BinaryWriter writer)
        {
            RefEntry = new RefTableEntry(this);
            ParamFile.RefEntries.Add(RefEntry);//reserve a space in the file's RefEntries so they stay in order

            var start = writer.BaseStream.Position - 1;
            writer.Write(Nodes.Count);

            ParamFile.UnresolvedStructs.Add(new Tuple<int, ParamStruct>((int)writer.BaseStream.Position, this));
            writer.Write((int)0);

            foreach (var node in Nodes.OrderBy(x => x.Key))
            {
                int hashIndex = ParamFile.AsmHashTable.IndexOf(node.Key);
                int relOffset = (int)(writer.BaseStream.Position - start);
                RefEntry.HashOffsets.Add(hashIndex, relOffset);

                Util.WriteParam(node.Value, writer, RefEntry);
            }
        }
    }
}
