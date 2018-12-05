using System;
using System.IO;
using System.Collections.Generic;

namespace paracobNET
{
    public class ParamFile
    {
        const string magic = "paracobn";

        public ParamStruct Root { get; private set; }

        static internal HashEntry[] HashData { get; private set; }
        static internal bool sortStruct { get; set; } = false;
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

        public ParamFile(string filepath, bool sortStruct)
        {
            ParamFile.sortStruct = sortStruct;
            Initialize(filepath);
        }
        public ParamFile(string filepath)
        {
            Initialize(filepath);
        }
        public void Initialize(string filepath)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
            {
                if (Util.ReadStringDirect(reader, 8) != magic)
                    throw new InvalidDataException("File contains an invalid header");
                HashTableSize = reader.ReadUInt32();
                RefTableSize = reader.ReadUInt32();
                HashData = new HashEntry[HashTableSize / 8];
                for (int i = 0; i < HashData.Length; i++)
                    HashData[i] = new HashEntry(reader.ReadUInt64());
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

        public void ReadLabels(string filepath)
        {
            Dictionary<uint, string> labels = new Dictionary<uint, string>();
            foreach (var line in File.ReadAllLines(filepath))
            {
                string[] splits = line.Split(',');
                try
                {
                    if (splits[0].Substring(0, 2) == "0x")
                        labels.Add(uint.Parse(splits[0].Substring(2), System.Globalization.NumberStyles.HexNumber),
                            splits[1]);
                    else throw new InvalidDataException();
                }
                catch { Console.WriteLine($"Parse error in {filepath}, \"{line}\""); }
            }
            foreach (var hash in HashData)
            {
                if (labels.ContainsKey(hash.Hash))
                    hash.Name = labels[hash.Hash];
                else
                    hash.Name = "";
            }
        }
    }
}
