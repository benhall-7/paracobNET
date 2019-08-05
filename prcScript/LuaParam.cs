using NLua;
using paracobNET;

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

        internal LuaParam(IParam inner)
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

        public LuaTable totable()
        {
            var t = newtable();
            t["type"] = type;
            switch (Inner.TypeKey)
            {
                case ParamType.@struct:
                    {
                        var s = Inner as ParamStruct;
                        int lua_i = 1;
                        foreach (var n in s.Nodes)
                        {
                            var t2 = newtable();
                            t2["hash"] = n.Key;
                            t2["param"] = new LuaParam(n.Value);
                            t[lua_i++] = t2;
                        }
                    }
                    break;
                case ParamType.list:
                    {
                        var l = Inner as ParamList;
                        int lua_i = 1;
                        foreach (var n in l.Nodes)
                            t[lua_i++] = new LuaParam(n);
                    }
                    break;
                default:
                    t["value"] = value;
                    break;
            }
            return t;
        }

        public LuaParam copy()
        {
            return new LuaParam(Inner.Clone());
        }

        private LuaTable newtable()
        {
            return Program.L.DoString(Resources.LuaNewTable)[0] as LuaTable;
        }
    }
}
