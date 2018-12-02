using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNET
{
    public class HashEntry
    {
        public uint Hash { get; internal set; }
        public uint Length { get; internal set; }
        public string Name { get; set; } = "";

        public bool IsNameReal
        {
            get { return Hash == Util.CRC32(Name.ToLower()); }
        }
    }
}
