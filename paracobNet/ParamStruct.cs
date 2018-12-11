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
        
        public StructNode[] Nodes { get; private set; }

        internal void Read()
        {
            var reader = ParamFile.Reader;
            uint startPos = (uint)reader.BaseStream.Position - 1;
            Nodes = new StructNode[reader.ReadUInt32()];
            reader.BaseStream.Seek(reader.ReadUInt32() + ParamFile.RefStart, SeekOrigin.Begin);
            Dictionary<uint, uint> pairs = new Dictionary<uint, uint>();
            for (int i = 0; i < Nodes.Length; i++)
                pairs.Add(reader.ReadUInt32(), reader.ReadUInt32());
            var hashIndeces = pairs.Keys.ToList();
            hashIndeces.Sort();
            for (int i = 0; i < Nodes.Length; i++)
            {
                reader.BaseStream.Seek(startPos + pairs[hashIndeces[i]], SeekOrigin.Begin);
                Nodes[i] = new StructNode(ParamFile.DisasmHashTable[hashIndeces[i]], reader);
            }
        }
        internal void Write()
        {
            uint startPos = (uint)ParamFile.WriterParam.BaseStream.Position - 1;
            //hashes are written in order of the nodes appearance in the class
            foreach (var node in Nodes)
            {
                Util.WriteHash(node.HashEntry);
                Util.WriteParam(node.Node);
            }
            SortedDictionary<HashEntry, IParam> sortedNodes = new SortedDictionary<HashEntry, IParam>();
            foreach (var node in Nodes)
                sortedNodes.Add(node.HashEntry, node.Node);
            //nodes are written in order of their hash40 values
            foreach (var node in sortedNodes)
            {

            }
        }

        public IParam GetNode(uint hash)
        {
            foreach (var node in Nodes)
                if (node.HashEntry.Hash == hash)
                    return node.Node;
            return null;
        }

        public class StructNode
        {
            public HashEntry HashEntry { get; private set; }
            public IParam Node { get; private set; }

            public StructNode(HashEntry hash, BinaryReader reader)
            {
                HashEntry = hash;
                Node = Util.ReadParam();
            }
        }
    }
}
