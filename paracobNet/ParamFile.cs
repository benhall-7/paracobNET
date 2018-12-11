using System;
using System.IO;
using System.Collections.Generic;

namespace paracobNET
{
    public class ParamFile
    {
        const string magic = "paracobn";

        public ParamStruct Root { get; private set; }

        #region global_disasm
        static internal BinaryReader reader { get; set; }
        static internal HashEntry[] AllHashes { get; private set; }
        static internal uint HashTableSize { get; set; }
        static internal uint RefTableSize { get; set; }
        static internal uint HashStart { get { return 0x10; } }
        static internal uint RefStart { get { return 0x10 + HashTableSize; } }
        static internal uint ParamStart { get { return 0x10 + HashTableSize + RefTableSize; } }
        #endregion

        #region global_asm
        //once each is finished being written, append to each other and write to file
        static internal MemoryStream writerHeader { get; set; }//header stream
        static internal MemoryStream writerTable { get; set; }//hash table and reference table stream
        static internal MemoryStream writerParam { get; set; }//param stream
        #endregion

        public ParamFile(string filepath)
        {
            using (reader = new BinaryReader(File.OpenRead(filepath)))
            {
                for (int i = 0; i < magic.Length; i++)
                    if (reader.ReadByte() != (byte)magic[i])
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
                    Root.Read();
                }
                else
                    throw new InvalidDataException("File does not have a root");
            }
            AllHashes = null;
        }

        public void Save(string filepath)
        {
            using (writerHeader = new MemoryStream())
            using (writerTable = new MemoryStream())
            using (writerParam = new MemoryStream())
            {
                for (int i = 0; i < 8; i++)
                    writerHeader.WriteByte((byte)magic[i]);
            }
        }
    }
}
