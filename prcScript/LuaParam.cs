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
                return (Inner as ParamValue).Value;
            }
            set
            {
                if (Inner.TypeKey == ParamType.@struct || Inner.TypeKey == ParamType.list)
                    return;
                (Inner as ParamValue).SetValue(value);
            }
        }

        public string value_string
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

        public LuaParam by_hash(ulong hash)
        {
            if (Inner is ParamStruct s)
                return new LuaParam(s.Nodes[hash]);
            return null;
        }

        public LuaParam by_label(string label)
        {
            if (Inner is ParamStruct s)
            {
                IParam child;
                //if the label isn't in the dictionary, catch it and convert directly to hash40 instead
                try { child = s.Nodes[label, Program.StringToHashLabels]; }
                catch (InvalidLabelException)
                {
                    //if the string as hash40 isn't present it will throw an exception
                    //stating that the string is not found, which is good behavior!
                    child = s.Nodes[label];
                }
                return new LuaParam(child);
            }
            return null;
        }

        public LuaParam by_index(int index)
        {
            switch (Inner.TypeKey)
            {
                case ParamType.@struct:
                    return new LuaParam((Inner as ParamStruct).Nodes[index].Value);
                case ParamType.list:
                    return new LuaParam((Inner as ParamList).Nodes[index]);
            }
            return null;
        }

        public LuaTable to_table()
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

        public LuaParam clone()
        {
            return new LuaParam(Inner.Clone());
        }

        private LuaTable newtable()
        {
            return Program.L.DoString(Resources.LuaNewTable)[0] as LuaTable;
        }
    }
}
