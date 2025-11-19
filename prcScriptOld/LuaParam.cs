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
                if (Inner.TypeKey == ParamType.Map || Inner.TypeKey == ParamType.Vec)
                    return null;
                return (Inner as ParamValue).Value;
            }
            set
            {
                if (Inner.TypeKey == ParamType.Map || Inner.TypeKey == ParamType.Vec)
                    return;
                (Inner as ParamValue).SetValue(value);
            }
        }

        public string value_string
        {
            get
            {
                if (Inner.TypeKey == ParamType.Map || Inner.TypeKey == ParamType.Vec)
                    return null;
                return (Inner as ParamValue).ToString(Program.HashToStringLabels);
            }
            set
            {
                if (Inner.TypeKey == ParamType.Map || Inner.TypeKey == ParamType.Vec)
                    return;
                (Inner as ParamValue).SetValue(value.ToString(), Program.StringToHashLabels);
            }
        }

        internal LuaParam(IParam inner)
        {
            Inner = inner;
        }

        public void by_hash(ulong hash, out LuaParam param, out string err)
        {
            param = null;
            err = null;
            if (Inner is ParamStruct s)
            {
                try { param = new LuaParam(s.Nodes[hash]); }
                catch { err = $"Unable to get param with hash 0x{hash:x8}"; }
                return;
            }
            err = "Unable to index param by hash; must be a ParamStruct";
        }

        public void by_label(string label, out LuaParam param, out string err)
        {
            param = null;
            err = null;
            if (Inner is ParamStruct s)
            {
                IParam child;
                //if the label isn't in the dictionary, catch it and convert directly to hash40 instead
                try
                {
                    child = s.Nodes[label, Program.StringToHashLabels];
                    param = new LuaParam(child);
                }
                catch
                {
                    err = $"Unable to index by given label '{label}'\n" +
                        "Either labels are not loaded, or they do not include this string\n" +
                        "Try adding the label, or indexing by its hash instead, using 'hash'.";
                }
                return;
            }
            err = "Unable to index param by label; must be a ParamStruct";
        }

        public void by_index(int index, out LuaParam param, out string err)
        {
            param = null;
            err = null;
            try
            {
                switch (Inner.TypeKey)
                {
                    case ParamType.Map:
                        param = new LuaParam((Inner as ParamStruct).Nodes[index].Value);
                        break;
                    case ParamType.Vec:
                        param = new LuaParam((Inner as ParamList).Nodes[index]);
                        break;
                    default:
                        err = "Unable to index param by position; must be a ParamStruct or a ParamList";
                        break;
                }
                return;
            }
            catch
            {
                err = $"Unable to index param with given value '{index}'";
                return;
            }
        }

        public LuaTable to_table()
        {
            var t = Resources.NewTable();
            t["type"] = type;
            switch (Inner.TypeKey)
            {
                case ParamType.Map:
                    {
                        var s = Inner as ParamStruct;
                        int lua_i = 1;
                        foreach (var n in s.Nodes)
                        {
                            var t2 = Resources.NewTable();
                            t2["hash"] = n.Key;
                            t2["param"] = new LuaParam(n.Value);
                            t[lua_i++] = t2;
                        }
                    }
                    break;
                case ParamType.Vec:
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
    }
}
