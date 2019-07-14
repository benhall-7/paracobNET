using System;
using System.Collections.Generic;
using System.IO;

namespace paracobNET
{
    public class ParamFile
    {
        public const string Magic = "paracobn";
        public ParamStruct Root { get; set; }

        #region ASSEMBLY_VARS

        internal static FileStream FileStream { get; set; }
        internal static BinaryWriter WriterHeader { get; set; }//header stream
        internal static List<ulong> AsmHashTable { get; set; }//list of hashes appended to
        internal static BinaryWriter WriterHash { get; set; }//hash table stream
        internal static List<object> AsmRefEntries { get; set; }//reference entry classes, and strings
        internal static BinaryWriter WriterRef { get; set; }//reference table stream
        internal static BinaryWriter WriterParam { get; set; }//param stream
        internal static List<Tuple<int, ParamStruct>> UnresolvedStructs { get; set; }

        internal static List<Tuple<int, string>> UnresolvedStrings { get; set; }
        internal static Dictionary<string, int> RefStringEntries { get; set; }
        
        #endregion

        public ParamFile(string filepath)
        {
            Disassembler disassembler = new Disassembler(filepath, out var root);
            Root = root;
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
                        WriterHeader.Write((byte)Magic[i]);
                    Util.WriteHash(0);
                    Util.IterateHashes(Root);

                    Util.WriteParam(Root, WriterParam);

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
