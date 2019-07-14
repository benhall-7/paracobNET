using System;
using System.Collections.Generic;
using System.IO;

namespace paracobNET
{
    [Serializable]
    public class ParamList : IParam
    {
        public ParamType TypeKey { get; } = ParamType.list;
        
        public List<IParam> Nodes { get; set; }

        public ParamList() { }
        public ParamList(List<IParam> nodes)
        {
            Nodes = nodes;
        }

        internal void Write(BinaryWriter writer)
        {
            uint startPos = (uint)writer.BaseStream.Position - 1;

            int count = Nodes.Count;
            writer.Write(count);

            uint[] offsets = new uint[Nodes.Count];
            long ptrStartPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(4 * count, SeekOrigin.Current);
            for (int i = 0; i < count; i++)
            {
                var node = Nodes[i];
                offsets[i] = (uint)(writer.BaseStream.Position - startPos);
                Util.WriteParam(node, writer);
            }
            long endPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(ptrStartPos, SeekOrigin.Begin);
            foreach (var offset in offsets)
                writer.Write(offset);
            writer.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }

        public IParam Clone()
        {
            ParamList clone = new ParamList();
            clone.Nodes = new List<IParam>(Nodes.Count);
            foreach (var node in Nodes)
                clone.Nodes.Add(node.Clone());
            return clone;
        }
    }
}
