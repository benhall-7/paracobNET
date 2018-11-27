using System;
using System.IO;
/*
check magic,
hashTableSize,
otherTableSize
go to 0x10 + hashTableSize + otherTableSize
label:Start
read command (switch)
if the cmd is not a Group, it must be contained by another Group
	add the command to the containing group and increment the object counter.
	If no parent group exists it will throw an exception (needs root node)
if the cmd is a Group, record the count
	go to offset
	for (int i = 0; i < count; i++) {
		get hash from first index (asynchronous read)
		go to second offset (in order to move the read pointer and enter the loop again, record current address for later)
		begin label:Start once more
		move pointer back to correct address
	}
*/
namespace paracobNet
{
    public class ParamFile
    {
        const string magic = "paracobn";

        public ParamStruct Root { get; private set; }
        internal uint HashTableSize { get; set; }
        internal uint RefTableSize { get; set; }
        public uint RefStart
        {
            get { return 0x10 + HashTableSize; }
        }
        public uint ParamStart
        {
            get { return 0x10 + HashTableSize + RefTableSize; }
        }
        //possibly add the hashes themselves if we can't reconstruct it completely

        public ParamFile(string filepath)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
            {
                if (Util.ReadString(reader, 8) != magic)
                    throw new InvalidDataException("File contains an invalid header");
                HashTableSize = reader.ReadUInt32();
                RefTableSize = reader.ReadUInt32();
                reader.BaseStream.Seek(ParamStart, SeekOrigin.Begin);

                if ((ParamType)reader.ReadByte() == ParamType.structure)
                    Root = new ParamStruct(reader, this);
                else
                    throw new InvalidDataException("File does not have a root");
            }
        }
    }
}
