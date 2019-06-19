using System;
using System.Collections.Generic;

namespace paracobNET
{
    [Serializable]
    public class Hash40Dictionary<T> : OrderedDictionary<ulong, T>
    {
        public Hash40Dictionary() : base() { }
        public Hash40Dictionary(int capacity) : base(capacity) { }

        public T this[string keyName]
        {
            get
            {
                try { return this[Hash40Util.StringToHash40(keyName)]; }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException($"The given key '{keyName}' was not present in the dictionary.");
                }
            }
            set
            {
                try { this[Hash40Util.StringToHash40(keyName)] = value; }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException($"The given key '{keyName}' was not present in the dictionary.");
                }
            }
        }
        public T this[string keyName, Dictionary<string, ulong> labels]
        {
            get
            {
                try { return this[Hash40Util.LabelToHash40(keyName, labels)]; }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException($"The given key '{keyName}' was not present in the dictionary.");
                }
            }
            set
            {
                try { this[Hash40Util.LabelToHash40(keyName, labels)] = value; }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException($"The given key '{keyName}' was not present in the dictionary.");
                }
            }
        }

        public bool ContainsKey(string keyName)
        {
            return ContainsKey(Hash40Util.StringToHash40(keyName));
        }
        public bool ContainsKey(string keyName, Dictionary<string, ulong> labels)
        {
            return ContainsKey(Hash40Util.LabelToHash40(keyName, labels));
        }

        public void Add(string keyName, T value)
        {
            try { Add(Hash40Util.StringToHash40(keyName), value); }
            catch (ArgumentException)
            {
                throw new ArgumentException($"An item with the key '{keyName}' has already been added");
            }
        }
        public void Add(string keyName, Dictionary<string, ulong> labels, T value)
        {
            try { Add(Hash40Util.LabelToHash40(keyName, labels), value); }
            catch (ArgumentException)
            {
                throw new ArgumentException($"An item with the key '{keyName}' has already been added");
            }
        }
    }
}
