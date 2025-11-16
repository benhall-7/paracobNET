namespace paracobNET
{
    internal class RefTableEntry
    {
        public int RefTableOffset { get; set; }
        public required List<KeyValuePair<int, int>> HashOffsets { get; set; }
        public required ParamMapNode CorrespondingStruct { get; set; }

        public RefTableEntry(ParamMapNode correspondingStruct)
        {
            CorrespondingStruct = correspondingStruct;
            HashOffsets = new List<KeyValuePair<int, int>>();
        }
        public RefTableEntry()
        {
            HashOffsets = new List<KeyValuePair<int, int>>();
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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
