using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace paracobNET
{
    internal class RefTableEntry : IEquatable<RefTableEntry>
    {
        public uint offset { get; set; }
        public Dictionary<uint, uint> hashOffsetPairs { get; set; }
        public Dictionary<string, uint> stringOffsetPairs { get; set; }
        public uint localStringOffset { get; set; }

        private uint localParamOffset { get; set; }

        public RefTableEntry(ParamStruct paramStruct)
        {
            hashOffsetPairs = new Dictionary<uint, uint>();
            stringOffsetPairs = new Dictionary<string, uint>();
            localParamOffset = 9;//typekey + count + offset
            localStringOffset = (uint)paramStruct.Nodes.Count * 8;
            foreach (var param in paramStruct.Nodes)
            {
                hashOffsetPairs.Add((uint)ParamFile.AsmHashTable.IndexOf(param.Key), localParamOffset);
                localParamOffset += Util.GetParamSize(param.Value);
            }
        }

        public void AppendString(string word)
        {
            if (!stringOffsetPairs.ContainsKey(word))
            {
                stringOffsetPairs.Add(word, localStringOffset);
                localStringOffset += (uint)word.Length + 1;
            }
        }

        public bool Equals(RefTableEntry other)
        {
            //thanks to Nick Jones: https://stackoverflow.com/a/3804852
            if (hashOffsetPairs.Count == other.hashOffsetPairs.Count && !hashOffsetPairs.Except(other.hashOffsetPairs).Any())
                return true;
            return false;
        }
    }
}
