using System.IO;

namespace paracobNET
{
    public class ParamArray : IParam
    {
        public ParamType TypeKey { get; } = ParamType.array;
        
        public IParam[] Nodes { get; private set; }

        internal void Read()
        {
            var reader = ParamFile.Reader;
            uint startPos = (uint)reader.BaseStream.Position - 1;
            Nodes = new IParam[reader.ReadUInt32()];
            uint[] offsets = new uint[Nodes.Length];

            //TODO: recognize a Type and make sure all elements are the same type

            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = reader.ReadUInt32();
            for (int i = 0; i < Nodes.Length; i++)
            {
                reader.BaseStream.Seek(startPos + offsets[i], SeekOrigin.Begin);
                Nodes[i] = Util.ReadParam();
            }
        }
        internal void Write()
        {
            var writer = ParamFile.WriterParam;
            uint startPos = (uint)writer.BaseStream.Position - 1;
            writer.Write(Nodes.Length);

            uint[] offsets = new uint[Nodes.Length];
            long ptrStartPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(4 * Nodes.Length, SeekOrigin.Current);
            for (int i = 0; i < Nodes.Length; i++)
            {
                var node = Nodes[i];
                offsets[i] = (uint)(writer.BaseStream.Position - startPos);
                Util.WriteParam(node);
            }
            long endPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(ptrStartPos, SeekOrigin.Begin);
            foreach (var offset in offsets)
                writer.Write(offset);
            writer.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }
    }
}
