using System;
using System.IO;
using System.Collections.Generic;

namespace paracobNET
{
    public class ParamFile
    {
        const string magic = "paracobn";

        public ParamStruct Root { get; private set; }

        static internal HashEntry[] AllHashes { get; private set; }
        static internal uint HashTableSize { get; set; }
        static internal uint RefTableSize { get; set; }
        static internal uint HashStart
        {
            get { return 0x10; }
        }
        static internal uint RefStart
        {
            get { return 0x10 + HashTableSize; }
        }
        static internal uint ParamStart
        {
            get { return 0x10 + HashTableSize + RefTableSize; }
        }
        
        public ParamFile(string filepath)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
            {
                if (Util.ReadStringDirect(reader, 8) != magic)
                    throw new InvalidDataException("File contains an invalid header");
                HashTableSize = reader.ReadUInt32();
                RefTableSize = reader.ReadUInt32();
                AllHashes = new HashEntry[HashTableSize / 8];
                for (int i = 0; i < AllHashes.Length; i++)
                    AllHashes[i] = new HashEntry(reader.ReadUInt64());
                reader.BaseStream.Seek(ParamStart, SeekOrigin.Begin);
                if ((ParamType)reader.ReadByte() == ParamType.structure)
                {
                    Root = new ParamStruct();
                    Root.Read(reader);
                }
                else
                    throw new InvalidDataException("File does not have a root");
            }
        }
    }
}
