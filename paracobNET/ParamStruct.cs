using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace paracobNET
{
    public class ParamStruct : IParam
    {
        public ParamType TypeKey { get; } = ParamType.@struct;
        public Hash40Dictionary<IParam> Nodes { get; set; }
        //this property only used when rebuilding
        internal RefTableEntry RefEntry { get; set; }

        public ParamStruct() { }
        public ParamStruct(Hash40Dictionary<IParam> nodes)
        {
            Nodes = nodes;
        }

        internal void Read(BinaryReader reader)
        {
            uint startPos = (uint)reader.BaseStream.Position - 1;
            int size = reader.ReadInt32();
            uint structRefOffset = reader.ReadUInt32();
            Nodes = new Hash40Dictionary<IParam>(size);

            Dictionary<int, int> hashOffsets;
            if (ParamFile.AsmRefEntries.TryGetValue(structRefOffset, out var refEntry))
                hashOffsets = refEntry.HashOffsets;
            else
            {
                var entry = new RefTableEntry();
                var hashOffsetTuples = new List<Tuple<int, int>>();
                reader.BaseStream.Seek(structRefOffset + ParamFile.RefStart, SeekOrigin.Begin);
                for (int i = 0; i < size; i++)
                {
                    int hashIndex = reader.ReadInt32();
                    int paramOffset = reader.ReadInt32();
                    hashOffsetTuples.Add(new Tuple<int, int>(hashIndex, paramOffset));
                    //entry.HashOffsets.Add(hashIndex, paramOffset);
                }
                //sort by the hash index, now we do this only once per RefEntry
                //TODO: maybe I should use a sorted dictionary instead?
                hashOffsetTuples.Sort((pair1, pair2) => pair1.Item1.CompareTo(pair2.Item1));
                foreach (var tuple in hashOffsetTuples)
                    entry.HashOffsets.Add(tuple.Item1, tuple.Item2);
                ParamFile.AsmRefEntries.Add(structRefOffset, entry);
                hashOffsets = entry.HashOffsets;
            }

            foreach (var pair in hashOffsets)
            {
                reader.BaseStream.Seek(startPos + pair.Value, SeekOrigin.Begin);
                ulong hash = ParamFile.DisasmHashTable[pair.Key];
                IParam param = Util.ReadParam(reader);
                Nodes.Add(hash, param);
            }
        }
        internal void Write(BinaryWriter writer)
        {
            RefEntry = new RefTableEntry(this);
            ParamFile.DisasmRefEntries.Add(RefEntry);//reserve a space in the file's RefEntries so they stay in order

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

        public IParam Clone()
        {
            ParamStruct clone = new ParamStruct();
            clone.Nodes = new Hash40Dictionary<IParam>(Nodes.Count);
            foreach (var node in Nodes)
                clone.Nodes.Add(node.Key, node.Value.Clone());
            return clone;
        }
    }
}
