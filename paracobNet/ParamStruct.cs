using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNet
{
    public class ParamStruct : IParam
    {
        public ParamType TypeKey { get; } = ParamType.structure;

        public ParamFile ParentFile { get; private set; }
        public IParam[] Nodes { get; private set; }

        public ParamStruct(BinaryReader reader, ParamFile file)// : base(PrmType.structure, reader, file)
        {
            Nodes = new IParam[reader.ReadUInt32()];
            uint reference = reader.ReadUInt32();
            for (int i = 0; i < Nodes.Length; i++)
            {
                reader.BaseStream.Seek(0x10 + file.HashTableSize + reference, SeekOrigin.Begin);

            }
        }

        public void Read(BinaryReader reader)
        {

        }

        //public Dictionary<byte, Type> 
    }
}
