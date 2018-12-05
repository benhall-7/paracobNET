using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNET
{
    public class HashEntry
    {
        public ulong Hash40 { get; internal set; }
        public string Name { get; set; } = "";

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
        public bool IsNameReal
        {
            get { return Hash == Util.CRC32(Name.ToLower()); }
        }

        public override string ToString()
        {
            if (Name.Length > 0)
                return Name;
            return Hash40.ToString("x10");
        }
    }
}
