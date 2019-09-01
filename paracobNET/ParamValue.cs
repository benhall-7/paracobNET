using System;
using System.Collections.Generic;

namespace paracobNET
{
    [Serializable]
    public class ParamValue : IParam
    {
        public ParamType TypeKey { get; private set; }
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

        public void SetValue(object value)
        {
            switch (TypeKey)
            {
                case ParamType.@bool:
                    Value = Convert.ToBoolean(value);
                    break;
                case ParamType.@sbyte:
                    Value = Convert.ToSByte(value);
                    break;
                case ParamType.@byte:
                    Value = Convert.ToByte(value);
                    break;
                case ParamType.@short:
                    Value = Convert.ToInt16(value);
                    break;
                case ParamType.@ushort:
                    Value = Convert.ToUInt16(value);
                    break;
                case ParamType.@int:
                    Value = Convert.ToInt32(value);
                    break;
                case ParamType.@uint:
                    Value = Convert.ToUInt32(value);
                    break;
                case ParamType.@float:
                    Value = Convert.ToSingle(value);
                    break;
                case ParamType.hash40:
                    Value = Convert.ToUInt64(value);
                    break;
                case ParamType.@string:
                    Value = Convert.ToString(value);
                    break;
            }
        }

        public void ChangeType(ParamType to)
        {
            ParamType from = TypeKey;
            if (from == to)
                return;
            if (to == ParamType.list || to == ParamType.@struct)
                throw new ArgumentException($"ParamType '{to.ToString()}' is invalid for ParamValue instances");
            else if (IsNumber(from) && IsNumber(to))
            {
                switch (to)
                {
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
                }
            }
            else
            {
                SetDefaultValue(to);
            }
            TypeKey = to;
        }

        private void SetDefaultValue(ParamType to)
        {
            switch (to)
            {
                case ParamType.@bool:
                    Value = false;
                    break;
                case ParamType.@sbyte:
                case ParamType.@byte:
                case ParamType.@short:
                case ParamType.@ushort:
                case ParamType.@int:
                case ParamType.@uint:
                case ParamType.@float:
                case ParamType.hash40:
                    Value = 0;
                    break;
                case ParamType.@string:
                    Value = Value.ToString();
                    break;
            }
        }

        private bool IsNumber(ParamType type)
        {
            switch (TypeKey)
            {
                case ParamType.@sbyte:
                case ParamType.@byte:
                case ParamType.@short:
                case ParamType.@ushort:
                case ParamType.@int:
                case ParamType.@uint:
                case ParamType.@float:
                    return true;
                default:
                    return false;
            }
        }

        public IParam Clone()
        {
            return new ParamValue(TypeKey, Value);
        }
    }
}
