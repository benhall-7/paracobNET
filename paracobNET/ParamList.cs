using System;
using System.Collections.Generic;

namespace paracobNET
{
    [Serializable]
    public class ParamList : IParam, IQueriable
    {
        public ParamType TypeKey { get; } = ParamType.list;
        
        public List<IParam> Nodes { get; set; }

        public ParamList()
        {
            Nodes = new List<IParam>();
        }
        public ParamList(int capacity)
        {
            Nodes = new List<IParam>(capacity);
        }
        public ParamList(List<IParam> nodes)
        {
            Nodes = nodes;
        }

        public IParam Clone()
        {
            ParamList clone = new ParamList(Nodes.Count);
            foreach (var node in Nodes)
                clone.Nodes.Add(node.Clone());
            return clone;
        }

        public QueryResult Query(string path)
        {
            throw new NotImplementedException();
        }

        public QueryResult Query(string path, IDictionary<string, ulong> labels)
        {
            throw new NotImplementedException();
        }
    }
}
