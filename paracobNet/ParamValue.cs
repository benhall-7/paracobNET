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
                case ParamType.@bool:
                    Value = reader.ReadByte() != 0;
                    break;
                case ParamType.@sbyte:
                    Value = reader.ReadSByte();
                    break;
                case ParamType.@byte:
                    Value = reader.ReadByte();
                    break;
                case ParamType.@short:
                    Value = reader.ReadInt16();
                    break;
                case ParamType.@ushort:
                    Value = reader.ReadUInt16();
                    break;
                case ParamType.@int:
                    Value = reader.ReadInt32();
                    break;
                case ParamType.@uint:
                    Value = reader.ReadUInt32();
                    break;
                case ParamType.@float:
                    Value = reader.ReadSingle();
                    break;
                case ParamType.hash40:
                    Value = ParamFile.DisasmHashTable[reader.ReadUInt32()];
                    break;
                case ParamType.@string:
                    Value = Util.ReadStringAsync(reader, ParamFile.RefStart + reader.ReadUInt32());
                    break;
            }
        }
        internal void Write()
        {
            var writer = ParamFile.WriterParam;
            switch (TypeKey)
            {
                case ParamType.@bool:
                    writer.Write((bool)Value);
                    break;
                case ParamType.@sbyte:
                    writer.Write((sbyte)Value);
                    break;
                case ParamType.@byte:
                    writer.Write((byte)Value);
                    break;
                case ParamType.@short:
                    writer.Write((short)Value);
                    break;
                case ParamType.@ushort:
                    writer.Write((ushort)Value);
                    break;
                case ParamType.@int:
                    writer.Write((int)Value);
                    break;
                case ParamType.@uint:
                    writer.Write((uint)Value);
                    break;
                case ParamType.@float:
                    writer.Write((float)Value);
                    break;
                case ParamType.hash40:
                    writer.Write(ParamFile.AsmHashTable.IndexOf((Hash40)Value));
                    break;
                case ParamType.@string:
                    writer.Write((uint)ParamFile.WriterRef.BaseStream.Position);
                    Util.WriteString((string)Value);
                    break;
            }
        }

        public string ToString(Dictionary<uint, string> labels)
        {
            if (TypeKey == ParamType.hash40)
                return ((Hash40)Value).ToString(labels);
            return Value.ToString();
        }
    }
}
