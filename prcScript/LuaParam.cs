using paracobNET;
using NLua;
using System;
using System.Collections.Generic;
using System.Text;

namespace prcScript
{
    public class LuaParam
    {
        internal IParam Inner { get; set; }

        public string type => Inner.TypeKey.ToString();

        public object value
        {
            get
            {
                if (Inner.TypeKey == ParamType.@struct || Inner.TypeKey == ParamType.list)
                    return null;
                return (Inner as ParamValue).ToString(Program.HashToStringLabels);
            }
            set
            {
                if (Inner.TypeKey == ParamType.@struct || Inner.TypeKey == ParamType.list)
                    return;
                (Inner as ParamValue).SetValue(value.ToString(), Program.StringToHashLabels);
            }
        }

        /// <summary>
        /// Child params as an array. First and last elements are null to work as lua tables
        /// </summary>
        public LuaParam[] children
        {
            get
            {
                switch (Inner.TypeKey)
                {
                    case ParamType.@struct:
                        {
                            var s = Inner as ParamStruct;
                            var prms = new LuaParam[s.Nodes.Count + 2];
                            for (int i = 0; i < s.Nodes.Count; i++)
                            {
                                prms[i + 1] = new LuaParam(s.Nodes[i]);
                            }
                            return prms;
                        }
                    case ParamType.list:
                        {
                            var l = Inner as ParamList;
                            var prms = new LuaParam[l.Nodes.Count + 2];
                            for (int i = 0; i < l.Nodes.Count; i++)
                            {
                                prms[i + 1] = new LuaParam(l.Nodes[i]);
                            }
                            return prms;
                        }
                }
                return null;
            }
        }

        public LuaParam(IParam inner)
        {
            Inner = inner;
        }

        public LuaParam get_child(object indexer)
        {
            switch (Inner.TypeKey)
            {
                case ParamType.@struct:
                    {
                        var s = Inner as ParamStruct;
                        if (indexer is long hash)
                            return new LuaParam(s.Nodes[(ulong)hash]);
                        else if (indexer is string label)
                        {
                            try { return new LuaParam(s.Nodes[label, Program.StringToHashLabels]); }
                            catch (InvalidLabelException)
                            {
                                return new LuaParam(s.Nodes[label]);
                            }
                        }
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

        public void set_child(object indexer, LuaParam child)
        {

        }

        public IParam Clone()
        {
            return Inner.Clone();
        }
    }
}
