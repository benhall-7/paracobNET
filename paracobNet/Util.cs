using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNet
{
    static class Util
    {
        public static IParam Read(BinaryReader reader, ParamFile file)
        {
            byte key = reader.ReadByte();
            if (!Enum.IsDefined(typeof(ParamType), key))
                throw new NotImplementedException($"Unimplemented param type '{key}' at {reader.BaseStream.Position}");
            ParamType type = (ParamType)key;
            switch (type)
            {
                case ParamType.array:
                    return new ParamArray(reader);
                case ParamType.structure:
                    return new ParamStruct(reader, file);
                default:
                    return new ParamValue(reader, type);
            }
        }
        public static string ReadString(BinaryReader reader, uint len)
        {
            string s = "";
            for (int i = 0; i < len; i++)
                s += reader.ReadChar();
            return s;
        }
        public static string ReadStringAsync(BinaryReader reader, uint position)
        {
            long returnTo = reader.BaseStream.Position;
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            string s = ""; char c;
            while ((c = reader.ReadChar()) != 0)
                s += c;
            reader.BaseStream.Seek(returnTo, SeekOrigin.Begin);
            return s;
        }
    }
}
