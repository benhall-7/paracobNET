using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
            uint reference = reader.ReadUInt32() + ParamFile.RefStart;
            for (int i = 0; i < Nodes.Length; i++)
            {
                reader.BaseStream.Seek(reference + 8 * i, SeekOrigin.Begin);
                uint hashIndex = reader.ReadUInt32();
                //go to the node offset and read the param
                reader.BaseStream.Seek(startPos + reader.ReadUInt32(), SeekOrigin.Begin);

                Nodes[i] = new StructNode(hashIndex, reader);
            }
        }

        public StructNode[] GetNodes(ParamType type)
        {
            List<StructNode> list = new List<StructNode>();
            foreach (var node in Nodes)
                if (node.Node.TypeKey == type)
                    list.Add(node);
            return list.ToArray();
        }

        public class StructNode
        {
            public uint HashIndex { get; private set; }
            public IParam Node { get; private set; }

            public StructNode(uint hashIndex, BinaryReader reader)
            {
                HashIndex = hashIndex;
                Node = Util.ReadParam(reader);
            }
        }
    }
}
