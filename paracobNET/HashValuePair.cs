using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNET
{
    [Serializable]
    public class HashValuePair<T>
    {
        public ulong Key { get; set; }
        public T Value { get; set; }

        public HashValuePair(ulong key, T value)
        {
            Key = key;
            Value = value;
        }
    }
}
