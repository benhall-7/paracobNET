using paracobNET;
using NLua;
using System;
using System.Collections.Generic;
using System.Text;

namespace prcScript
{
    public class LuaParam : IParam
    {
        IParam Inner { get; set; }

        public ParamType TypeKey => Inner.TypeKey;

        public LuaParam(IParam inner)
        {
            Inner = inner;
        }

        public LuaParam get(object indexer)
        {
            switch (Inner.TypeKey)
            {
                case ParamType.@struct:
                    {
                        var s = Inner as ParamStruct;
                        if (indexer is ulong hash)
                            return new LuaParam(s.Nodes[hash]);
                        else if (indexer is string label)
                            return new LuaParam(s.Nodes[label]);
                        else
                            return new LuaParam(s.Nodes[(int)indexer]);
                    }
                case ParamType.list:
                    {
                        var l = Inner as ParamList;
                        return new LuaParam(l.Nodes[(int)indexer]);
                    }
            }
            return null;
        }

        public LuaParam get(object indexer, object labels)
        {
            switch (Inner.TypeKey)
            {
                case ParamType.@struct:
                    break;
                case ParamType.list:
                    {
                        var l = Inner as ParamList;
                        return new LuaParam(l.Nodes[(int)indexer]);
                    }
            }
            return null;
        }

        public void save(string path)
        {

        }

        public IParam Clone()
        {
            return Inner.Clone();
        }
    }
}
