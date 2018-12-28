using System;
using System.Collections.Generic;

namespace paracobNET
{
    public struct Hash40 : IComparable<Hash40>
    {
        public ulong Value { get; internal set; }

        public Hash40(ulong hash40)
        {
            Value = hash40;
        }
        public Hash40(string hash40, Dictionary<string, ulong> labels)
        {
            if (hash40.StartsWith("0x"))
                Value = ulong.Parse(hash40.Substring(2), System.Globalization.NumberStyles.HexNumber);
            else if (labels.ContainsKey(hash40))
                Value = labels[hash40];
            else
                throw new Exception("The string " + hash40 + " does not represent a hexadecimal value and no matching label was found");
        }

        public uint Hash
        {
            get { return (uint)Value; }
        }
        public byte Length
        {
            get { return (byte)(Value >> 32); }
        }

        public override string ToString()
        {
            return "0x" + Value.ToString("x10");
        }
        public string ToString(Dictionary<ulong, string> labels)
        {
            if (labels.ContainsKey(Value))
                return labels[Value];
            else return ToString();
        }

        public int CompareTo(Hash40 other)
        {
            if (Value > other.Value) return 1;
            if (Value == other.Value) return 0;
            return -1;
        }
    }
}
