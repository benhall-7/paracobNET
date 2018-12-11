using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNET
{
    public class ParamArray : IParam
    {
        public ParamType TypeKey { get; } = ParamType.array;
        
        public IParam[] Nodes { get; private set; }

        internal void Read()
        {
            var reader = ParamFile.reader;
            uint startPos = (uint)reader.BaseStream.Position - 1;
            Nodes = new IParam[reader.ReadUInt32()];
            uint[] offsets = new uint[Nodes.Length];

            //TODO: recognize a Type and make sure all elements are the same type

            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = reader.ReadUInt32();
            for (int i = 0; i < Nodes.Length; i++)
            {
                reader.BaseStream.Seek(startPos + offsets[i], SeekOrigin.Begin);
                Nodes[i] = Util.ReadParam(reader);
            }
        }
        internal void Write()
        {

        }
    }
}
