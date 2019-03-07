using System.IO;

namespace paracobNET
{
    public abstract class ParamBase
    {
        public abstract ParamType TypeKey { get; }

        internal abstract void Read(BinaryReader reader);
        internal abstract void Write(BinaryWriter writer);
    }
}
