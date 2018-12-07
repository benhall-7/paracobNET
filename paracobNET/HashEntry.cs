using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNET
{
    public struct HashEntry
    {
        public ulong Hash40 { get; internal set; }

        public HashEntry(ulong hash40)
        {
            Hash40 = hash40;
        }

        public uint Hash
        {
            get { return (uint)Hash40; }
        }
        public byte Length
        {
            get { return (byte)(Hash >> 32); }
        }

        public override string ToString()
        {
            return "0x" + Hash40.ToString("x10");
        }
        public string ToString(Dictionary<uint, string> labels)
        {
            if (labels.ContainsKey(Hash))
                return labels[Hash];
            else return ToString();
        }
    }
}
