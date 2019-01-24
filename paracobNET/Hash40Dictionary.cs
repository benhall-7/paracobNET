using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNET
{
    public class Hash40Dictionary<T> : Dictionary<ulong, T>
    {
        public bool ContainsKey(string keyName)
        {
            return ContainsKey(Hash40Util.StringToHash40(keyName));
        }

        public T this[string keyName]
        {
            get { return this[Hash40Util.StringToHash40(keyName)]; }
            set { this[Hash40Util.StringToHash40(keyName)] = value; }
        }
    }
}
