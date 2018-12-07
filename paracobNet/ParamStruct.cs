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

        public void Read(BinaryReader reader)
        {
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
                Nodes[i] = new StructNode(ParamFile.AllHashes[hashIndeces[i]], reader);
            }
        }

        //public StructNode[] GetNodes(ParamType type)
        //{
        //    List<StructNode> list = new List<StructNode>();
        //    foreach (var node in Nodes)
        //        if (node.Node.TypeKey == type)
        //            list.Add(node);
        //    return list.ToArray();
        //}

        public class StructNode
        {
            public HashEntry HashEntry { get; private set; }
            public IParam Node { get; private set; }

            public StructNode(HashEntry hash, BinaryReader reader)
            {
                HashEntry = hash;
                Node = Util.ReadParam(reader);
            }
        }
    }
}
