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
        public string ToString(Dictionary<uint, string> labels)
        {
            if (labels.ContainsKey(Hash))
                return labels[Hash];
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
