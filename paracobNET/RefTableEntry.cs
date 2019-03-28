using System;
using System.Collections.Generic;
using System.Linq;

namespace paracobNET
{
    internal class RefTableEntry : IEquatable<RefTableEntry>
    {
        public int RefTableOffset { get; set; }
        public Dictionary<int, int> HashOffsets { get; set; }
        public Dictionary<string, int> StringOffsets { get; set; }
        //used on rebuild, when each struct is given its own table entry
        public ParamStruct CorrespondingStruct { get; set; }

        public RefTableEntry(ParamStruct correspondingStruct)
        {
            CorrespondingStruct = correspondingStruct;
            HashOffsets = new Dictionary<int, int>();
            StringOffsets = new Dictionary<string, int>();
        }
        public RefTableEntry()
        {
            HashOffsets = new Dictionary<int, int>();
            StringOffsets = new Dictionary<string, int>();
        }

        public void AppendString(string word)
        {
            if (!StringOffsets.ContainsKey(word))
                StringOffsets.Add(word, 0);
        }

        public bool Equals(RefTableEntry other)
        {
            //thanks to Nick Jones: https://stackoverflow.com/a/3804852
            if (HashOffsets.Count == other.HashOffsets.Count && !HashOffsets.Except(other.HashOffsets).Any())
                return true;
            return false;
        }
    }
}
