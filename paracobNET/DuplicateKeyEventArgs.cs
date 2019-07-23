using System;

namespace paracobNET
{
    public class DuplicateKeyEventArgs : EventArgs
    {
        public ulong Hash { get; set; }

        public DuplicateKeyEventArgs(ulong hash)
        {
            Hash = hash;
        }
    }
}
