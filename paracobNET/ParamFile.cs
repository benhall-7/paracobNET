using System;
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
        static internal ulong[] DisasmHashTable { get; set; }
        static internal uint HashTableSize { get; set; }
        static internal uint RefTableSize { get; set; }
        static internal uint HashStart { get { return 0x10; } }
        static internal uint RefStart { get { return 0x10 + HashTableSize; } }
        static internal uint ParamStart { get { return 0x10 + HashTableSize + RefTableSize; } }
        static internal Dictionary<uint, RefTableEntry> DisasmRefEntries { get; set; }
        #endregion

        #region global_asm
        static internal FileStream FileStream { get; set; }
        static internal BinaryWriter WriterHeader { get; set; }//header stream
        static internal List<ulong> AsmHashTable { get; set; }//list of hashes appended to
        static internal BinaryWriter WriterHash { get; set; }//hash table stream
        static internal List<object> AsmRefEntries { get; set; }//reference entry classes, and strings
        static internal BinaryWriter WriterRef { get; set; }//reference table stream
        static internal BinaryWriter WriterParam { get; set; }//param stream
        static internal List<Tuple<int, ParamStruct>> UnresolvedStructs { get; set; }
        static internal List<Tuple<int, string>> UnresolvedStrings { get; set; }
        static internal Dictionary<string, int> RefStringEntries { get; set; }
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
                    DisasmRefEntries = new Dictionary<uint, RefTableEntry>();
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
                DisasmRefEntries = null;
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
                AsmRefEntries = new List<object>();
                UnresolvedStructs = new List<Tuple<int, ParamStruct>>();
                UnresolvedStrings = new List<Tuple<int, string>>();
                RefStringEntries = new Dictionary<string, int>();

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

                    Util.WriteParam(Root, WriterParam, null);

                    Util.MergeRefTables();
                    Util.WriteRefTables();

                    Util.ResolveStructStringRefs();

                    WriterHeader.Write((uint)WriterHash.BaseStream.Length);
                    WriterHeader.Write((uint)WriterRef.BaseStream.Length);

                    foreach (var writer in new BinaryWriter[] { WriterHeader, WriterHash, WriterRef, WriterParam })
                    {
                        writer.BaseStream.Position = 0;
                        writer.BaseStream.CopyTo(FileStream);
                    }
                }
            }
            finally
            {
                AsmHashTable = null;
                AsmRefEntries = null;
                UnresolvedStructs = null;
                UnresolvedStrings = null;
                RefStringEntries = null;
            }
        }
    }
}
