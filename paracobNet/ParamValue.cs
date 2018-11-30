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

        public void Read(BinaryReader reader)
        {
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
                case ParamType.uint32_2:
                    Value = reader.ReadUInt32();
                    break;
                case ParamType.str:
                    Value = Util.ReadStringAsync(reader, ParamFile.RefStart + reader.ReadUInt32());
                    break;
            }
        }
    }
}
