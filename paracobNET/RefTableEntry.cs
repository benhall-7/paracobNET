using System;
using System.Collections.Generic;
using System.Linq;

namespace paracobNET
{
    internal class RefTableEntry
    {
        public int RefTableOffset { get; set; }
        public Dictionary<int, int> HashOffsets { get; set; }
        //used on rebuild, when each struct is given its own table entry
        public ParamStruct CorrespondingStruct { get; set; }

        public RefTableEntry(ParamStruct correspondingStruct)
        {
            CorrespondingStruct = correspondingStruct;
            HashOffsets = new Dictionary<int, int>();
        }
        public RefTableEntry()
        {
            HashOffsets = new Dictionary<int, int>();
        }

        public override bool Equals(object other)
        {
            if (!(other is RefTableEntry entry))
                return false;
            if (base.Equals(entry))
                return true;
            //thanks to Nick Jones: https://stackoverflow.com/a/3804852
            if (HashOffsets.Count == entry.HashOffsets.Count && !HashOffsets.Except(entry.HashOffsets).Any())
                return true;
            return false;
        }
    }
}
