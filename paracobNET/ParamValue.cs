using System;
using System.Collections.Generic;
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
        public ParamValue(ParamType type, object value)
        {
            TypeKey = type;
            Value = value;
        }

        internal void Read(BinaryReader reader)
        {
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
        internal void Write(BinaryWriter writer, RefTableEntry parent)
        {
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
                    writer.Write(ParamFile.AsmHashTable.IndexOf((ulong)Value));
                    break;
                case ParamType.@string:
                    string word = (string)Value;
                    ParamFile.UnresolvedStrings.Add(new Tuple<int, string>((int)writer.BaseStream.Position, word));

                    Util.AppendRefTableString(word);
                    writer.Write((int)0);
                    break;
            }
        }

        public string ToString(Dictionary<ulong, string> labels)
        {
            if (TypeKey == ParamType.hash40)
                return Hash40Util.FormatToString((ulong)Value, labels);
            return Value.ToString();
        }

        public void SetValue(string value, Dictionary<string, ulong> labels)
        {
            switch (TypeKey)
            {
                case ParamType.@bool:
                    Value = bool.Parse(value);
                    break;
                case ParamType.@sbyte:
                    Value = sbyte.Parse(value);
                    break;
                case ParamType.@byte:
                    Value = byte.Parse(value);
                    break;
                case ParamType.@short:
                    Value = short.Parse(value);
                    break;
                case ParamType.@ushort:
                    Value = ushort.Parse(value);
                    break;
                case ParamType.@int:
                    Value = int.Parse(value);
                    break;
                case ParamType.@uint:
                    Value = uint.Parse(value);
                    break;
                case ParamType.@float:
                    Value = float.Parse(value);
                    break;
                case ParamType.hash40:
                    Value = Hash40Util.LabelToHash40(value, labels);
                    break;
                case ParamType.@string:
                    Value = value;
                    break;
            }
        }

        public IParam Clone()
        {
            return new ParamValue(TypeKey, Value);
        }
    }
}
