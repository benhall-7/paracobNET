using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNet
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
                long returnTo = reader.BaseStream.Position;
                reader.BaseStream.Seek(ParamFile.HashStart + hashIndex * 8, SeekOrigin.Begin);
                uint hash = reader.ReadUInt32();
                uint len = reader.ReadUInt32();
                reader.BaseStream.Seek(returnTo, SeekOrigin.Begin);
                //go to the node offset and read the param
                reader.BaseStream.Seek(startPos + reader.ReadUInt32(), SeekOrigin.Begin);

                Nodes[i] = new StructNode(hash, len, reader);
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
            public string Name { get; set; } = "";//allows user setting in case no matching name is found

            public uint Hash { get; private set; }
            public uint StrLength { get; private set; }
            public IParam Node { get; private set; }

            public bool IsNameReal
            {
                get
                {
                    return Hash == Util.CRC32(Name.ToLower());
                }
            }

            public StructNode(uint hash, uint strLength, BinaryReader reader)
            {
                Hash = hash;
                StrLength = strLength;
                Node = Util.ReadParam(reader);
            }
        }
    }
}
