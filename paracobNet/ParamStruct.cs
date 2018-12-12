using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace paracobNET
{
    public class ParamStruct : IParam
    {
        public ParamType TypeKey { get; } = ParamType.structure;
        
        public Dictionary<HashEntry, IParam> Nodes { get; private set; }

        internal void Read()
        {
            var reader = ParamFile.Reader;
            uint startPos = (uint)reader.BaseStream.Position - 1;
            uint size = reader.ReadUInt32();
            Nodes = new Dictionary<HashEntry, IParam>();

            reader.BaseStream.Seek(reader.ReadUInt32() + ParamFile.RefStart, SeekOrigin.Begin);
            Dictionary<uint, uint> pairs = new Dictionary<uint, uint>();
            for (int i = 0; i < size; i++)
                pairs.Add(reader.ReadUInt32(), reader.ReadUInt32());
            var hashIndeces = pairs.Keys.ToList();
            hashIndeces.Sort();
            for (int i = 0; i < size; i++)
            {
                var key = hashIndeces[i];
                reader.BaseStream.Seek(startPos + pairs[key], SeekOrigin.Begin);
                Nodes.Add(ParamFile.DisasmHashTable[key], Util.ReadParam());
            }
        }
        internal void Write()
        {
            var paramWriter = ParamFile.WriterParam;
            var refWriter = ParamFile.WriterRef;
            uint[] offsets = new uint[Nodes.Count];
            long paramStartPos = paramWriter.BaseStream.Position - 1;
            long refStartPos = refWriter.BaseStream.Position;

            paramWriter.Write(Nodes.Count);
            paramWriter.Write((uint)refWriter.BaseStream.Position);

            List<HashEntry> sortedHashes = Nodes.Keys.ToList();
            sortedHashes.Sort();

            //allocate space for the entire node's contents first
            //so we can generate offsets when each one is assembled
            refWriter.BaseStream.Seek(Nodes.Count * 8, SeekOrigin.Current);
            for (int i = 0; i < Nodes.Count; i++)
            {
                offsets[i] = (uint)(paramWriter.BaseStream.Position - paramStartPos);
                Util.WriteParam(Nodes[sortedHashes[i]]);
            }
            refWriter.BaseStream.Seek(refStartPos, SeekOrigin.Begin);
            for (int i = 0; i < Nodes.Count; i++)
            {
                refWriter.Write(ParamFile.AsmHashTable.IndexOf(sortedHashes[i]));
                refWriter.Write(offsets[i]);
            }
        }

        public IParam GetNode(uint hash)
        {
            foreach (var node in Nodes)
                if (node.Key.Hash == hash)
                    return node.Value;
            return null;
        }

        //replace this with a dictionary later?
        //public class StructNode
        //{
        //    public HashEntry HashEntry { get; private set; }
        //    public IParam Node { get; private set; }

        //    public StructNode(HashEntry hash, BinaryReader reader)
        //    {
        //        HashEntry = hash;
        //        Node = Util.ReadParam();
        //    }
        //}
    }
}
