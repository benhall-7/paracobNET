using System;
using System.Collections.Generic;
using System.IO;

namespace paracobNET
{
    [Serializable]
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

        public string ToString(IDictionary<ulong, string> labels)
        {
            if (TypeKey == ParamType.hash40)
                return Hash40Util.FormatToString((ulong)Value, labels);
            return Value.ToString();
        }

        public void SetValue(string value, IDictionary<string, ulong> labels)
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

        public void SetType(ParamType type)
        {
            if (TypeKey == type)
                return;
            switch (type)
            {
                case ParamType.list:
                case ParamType.@struct:
                    throw new ArgumentException($"ParamType '{type.ToString()}' is invalid for ParamValue instances");
            }
            switch (TypeKey)
            {
                case ParamType.@bool:
                case ParamType.@sbyte:
                case ParamType.@byte:
                case ParamType.@short:
                case ParamType.@ushort:
                case ParamType.@int:
                case ParamType.@uint:
                case ParamType.@float:
                    try
                    {
                        switch (type)
                        {
                            case ParamType.@bool:
                                Value = (bool)Value;
                                break;
                            case ParamType.@sbyte:
                                Value = (sbyte)Value;
                                break;
                            case ParamType.@byte:
                                Value = (byte)Value;
                                break;
                            case ParamType.@short:
                                Value = (short)Value;
                                break;
                            case ParamType.@ushort:
                                Value = (ushort)Value;
                                break;
                            case ParamType.@int:
                                Value = (int)Value;
                                break;
                            case ParamType.@uint:
                                Value = (uint)Value;
                                break;
                            case ParamType.@float:
                                Value = (float)Value;
                                break;
                            default:
                                break;
                        }
                    }
                    catch
                    {
                        Value = 0;
                    }
                    break;
                case ParamType.hash40:
                    {
                        switch (type)
                        {
                            case ParamType.@bool:
                            case ParamType.@sbyte:
                            case ParamType.@byte:
                            case ParamType.@short:
                            case ParamType.@ushort:
                            case ParamType.@int:
                            case ParamType.@uint:
                            case ParamType.@float:
                                Value = 0;
                                break;
                            case ParamType.@string:
                                Value = "";
                                break;
                        }
                    }
                    break;
                case ParamType.@string:
                    {
                        try
                        {
                            string str = (string)Value;
                            switch (type)
                            {
                                case ParamType.@bool:
                                    Value = bool.Parse(str);
                                    break;
                                case ParamType.@sbyte:
                                    Value = sbyte.Parse(str);
                                    break;
                                case ParamType.@byte:
                                    Value = byte.Parse(str);
                                    break;
                                case ParamType.@short:
                                    Value = short.Parse(str);
                                    break;
                                case ParamType.@ushort:
                                    Value = ushort.Parse(str);
                                    break;
                                case ParamType.@int:
                                    Value = int.Parse(str);
                                    break;
                                case ParamType.@uint:
                                    Value = uint.Parse(str);
                                    break;
                                case ParamType.@float:
                                    Value = float.Parse(str);
                                    break;
                                case ParamType.hash40:
                                    Value = 0;
                                    break;
                            }
                        }
                        catch
                        {
                            Value = 0;
                        }
                    }
                    break;
            }
        }

        public IParam Clone()
        {
            return new ParamValue(TypeKey, Value);
        }
    }
}
