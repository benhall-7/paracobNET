using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNET
{
    public class ParamValue : IParam
    {
        public ParamType TypeKey { get; set; }
        public object Value { get; set; }

        public ParamValue(ParamType type)
        {
            TypeKey = type;
        }

        internal void Read()
        {
            var reader = ParamFile.Reader;
            switch (TypeKey)
            {
                case ParamType.boolean:
                    Value = reader.ReadByte() != 0;
                    break;
                case ParamType.int8:
                    Value = reader.ReadSByte();
                    break;
                case ParamType.uint8:
                    Value = reader.ReadByte();
                    break;
                case ParamType.int16:
                    Value = reader.ReadInt16();
                    break;
                case ParamType.uint16:
                    Value = reader.ReadUInt16();
                    break;
                case ParamType.int32:
                    Value = reader.ReadInt32();
                    break;
                case ParamType.uint32:
                    Value = reader.ReadUInt32();
                    break;
                case ParamType.float32:
                    Value = reader.ReadSingle();
                    break;
                case ParamType.hash40:
                    Value = ParamFile.DisasmHashTable[reader.ReadUInt32()];
                    break;
                case ParamType.str:
                    Value = Util.ReadStringAsync(reader, ParamFile.RefStart + reader.ReadUInt32());
                    break;
            }
        }
        internal void Write()
        {
            var writer = ParamFile.WriterParam;
            switch (TypeKey)
            {
                case ParamType.boolean:
                    writer.Write((bool)Value ? (byte)1 : (byte)0);
                    break;
                case ParamType.int8:
                    writer.Write((sbyte)Value);
                    break;
                case ParamType.uint8:
                    writer.Write((byte)Value);
                    break;
                case ParamType.int16:
                    writer.Write((short)Value);
                    break;
                case ParamType.uint16:
                    writer.Write((ushort)Value);
                    break;
                case ParamType.int32:
                    writer.Write((int)Value);
                    break;
                case ParamType.uint32:
                    writer.Write((uint)Value);
                    break;
                case ParamType.float32:
                    writer.Write((float)Value);
                    break;
                case ParamType.hash40:
                    Util.WriteHash((HashEntry)Value);
                    writer.Write(ParamFile.AsmHashTable.IndexOf((HashEntry)Value));
                    break;
                case ParamType.str:
                    writer.Write((uint)ParamFile.WriterRef.BaseStream.Position);
                    Util.WriteString((string)Value);
                    break;
            }
        }

        public string ToString(Dictionary<uint, string> labels)
        {
            if (TypeKey == ParamType.hash40)
                return ((HashEntry)Value).ToString(labels);
            return Value.ToString();
        }
    }
}
