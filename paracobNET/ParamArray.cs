using System.IO;

namespace paracobNET
{
    public class ParamArray : IParam
    {
        public ParamType TypeKey { get; } = ParamType.array;
        
        public IParam[] Nodes { get; private set; }

        public ParamArray() { }
        public ParamArray(IParam[] nodes)
        {
            Nodes = nodes;
        }

        internal void Read(BinaryReader reader)
        {
            uint startPos = (uint)reader.BaseStream.Position - 1;
            Nodes = new IParam[reader.ReadUInt32()];
            uint[] offsets = new uint[Nodes.Length];

            //all elements should be the same type but it's not enforced

            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = reader.ReadUInt32();
            for (int i = 0; i < Nodes.Length; i++)
            {
                reader.BaseStream.Seek(startPos + offsets[i], SeekOrigin.Begin);
                Nodes[i] = Util.ReadParam(reader);
            }
        }
        internal void Write(BinaryWriter writer, RefTableEntry parent)
        {
            uint startPos = (uint)writer.BaseStream.Position - 1;
            writer.Write(Nodes.Length);

            uint[] offsets = new uint[Nodes.Length];
            long ptrStartPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(4 * Nodes.Length, SeekOrigin.Current);
            for (int i = 0; i < Nodes.Length; i++)
            {
                var node = Nodes[i];
                offsets[i] = (uint)(writer.BaseStream.Position - startPos);
                Util.WriteParam(node, writer, parent);
            }
            long endPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(ptrStartPos, SeekOrigin.Begin);
            foreach (var offset in offsets)
                writer.Write(offset);
            writer.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }
    }
}
