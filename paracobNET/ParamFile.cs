using System.Collections.Generic;
using System.IO;

namespace paracobNET
{
    public class ParamFile
    {
        const string magic = "paracobn";

        public ParamStruct Root { get; private set; }

        #region global_disasm
        static internal BinaryReader Reader { get; set; }
        static internal ulong[] DisasmHashTable { get; private set; }
        static internal uint HashTableSize { get; set; }
        static internal uint RefTableSize { get; set; }
        static internal uint HashStart { get { return 0x10; } }
        static internal uint RefStart { get { return 0x10 + HashTableSize; } }
        static internal uint ParamStart { get { return 0x10 + HashTableSize + RefTableSize; } }
        static internal List<uint> StructOffsets { get; set; }
        #endregion

        #region global_asm
        //once each is finished being written, append each together and write to file
        static internal FileStream FileStream { get; set; }
        static internal BinaryWriter WriterHeader { get; set; }//header stream
        static internal List<ulong> AsmHashTable { get; set; }//list of hashes appended to
        static internal BinaryWriter WriterHash { get; set; }//hash table stream
        static internal List<RefTableEntry> RefEntries { get; set; }//reference entry classes
        static internal uint RefSize { get; set; }
        static internal BinaryWriter WriterRef { get; set; }//reference table stream
        static internal BinaryWriter WriterParam { get; set; }//param stream
        #endregion

        public ParamFile(string filepath)
        {
            try
            {
                using (Reader = new BinaryReader(File.OpenRead(filepath)))
                {
                    for (int i = 0; i < magic.Length; i++)
                        if (Reader.ReadByte() != (byte)magic[i])
                            throw new InvalidDataException("File contains an invalid header");

                    HashTableSize = Reader.ReadUInt32();
                    RefTableSize = Reader.ReadUInt32();
                    DisasmHashTable = new ulong[HashTableSize / 8];
                    for (int i = 0; i < DisasmHashTable.Length; i++)
                        DisasmHashTable[i] = Reader.ReadUInt64();
                    StructOffsets = new List<uint>();
                    Reader.BaseStream.Seek(ParamStart, SeekOrigin.Begin);
                    if ((ParamType)Reader.ReadByte() == ParamType.@struct)
                    {
                        Root = new ParamStruct();
                        Root.Read(Reader);
                    }
                    else
                        throw new InvalidDataException("File does not have a root");
                }
            }
            finally
            {
                DisasmHashTable = null;
                StructOffsets = null;
            }
        }
        public ParamFile(ParamStruct root)
        {
            Root = root;
        }

        public void Save(string filepath)
        {
            try
            {
                AsmHashTable = new List<ulong>();
                RefEntries = new List<RefTableEntry>();
                using (FileStream = File.Create(filepath))
                using (WriterHeader = new BinaryWriter(new MemoryStream()))
                using (WriterHash = new BinaryWriter(new MemoryStream()))
                using (WriterRef = new BinaryWriter(new MemoryStream()))
                using (WriterParam = new BinaryWriter(new MemoryStream()))
                {
                    for (int i = 0; i < 8; i++)
                        WriterHeader.Write((byte)magic[i]);
                    Util.WriteHash(0);
                    Util.IterateHashes(Root);
                    Root.SetupRefTable();
                    RefSize = 0;
                    foreach (var entry in RefEntries)
                    {
                        entry.offset = RefSize;
                        RefSize += entry.localStringOffset;
                    }
                    Util.WriteRefTables();
                    Util.WriteParam(Root, WriterParam);
                    WriterHeader.Write((uint)WriterHash.BaseStream.Length);
                    WriterHeader.Write(RefSize);

                    WriterHeader.BaseStream.Position = 0;
                    WriterHash.BaseStream.Position = 0;
                    WriterRef.BaseStream.Position = 0;
                    WriterParam.BaseStream.Position = 0;

                    WriterHeader.BaseStream.CopyTo(FileStream);
                    WriterHash.BaseStream.CopyTo(FileStream);
                    WriterRef.BaseStream.CopyTo(FileStream);
                    WriterParam.BaseStream.CopyTo(FileStream);
                }
            }
            finally
            {
                AsmHashTable = null;
                RefEntries = null;
            }
        }
    }
}
