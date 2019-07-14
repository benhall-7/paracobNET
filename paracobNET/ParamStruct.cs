using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace paracobNET
{
    [Serializable]
    public class ParamStruct : IParam
    {
        public ParamType TypeKey { get; } = ParamType.@struct;
        public Hash40Dictionary<IParam> Nodes { get; set; }

        //TODO: vvv REMOVE vvv
        //this property only used when rebuilding
        internal RefTableEntry RefEntry { get; set; }

        public ParamStruct() { }
        public ParamStruct(Hash40Dictionary<IParam> nodes)
        {
            Nodes = nodes;
        }

        internal void Read(BinaryReader reader)
        {
            
        }
        internal void Write(BinaryWriter writer)
        {
            RefEntry = new RefTableEntry(this);
            ParamFile.AsmRefEntries.Add(RefEntry);//reserve a space in the file's RefEntries so they stay in order

            var start = writer.BaseStream.Position - 1;
            writer.Write(Nodes.Count);

            ParamFile.UnresolvedStructs.Add(new Tuple<int, ParamStruct>((int)writer.BaseStream.Position, this));
            writer.Write(0);

            foreach (var node in Nodes.OrderBy(x => x.Key))
            {
                int hashIndex = ParamFile.AsmHashTable.IndexOf(node.Key);
                int relOffset = (int)(writer.BaseStream.Position - start);
                RefEntry.HashOffsets.Add(hashIndex, relOffset);

                Util.WriteParam(node.Value, writer);
            }
        }

        public IParam Clone()
        {
            ParamStruct clone = new ParamStruct();
            clone.Nodes = new Hash40Dictionary<IParam>(Nodes.Count);
            foreach (var node in Nodes)
                clone.Nodes.Add(node.Key, node.Value.Clone());
            return clone;
        }
    }
}
