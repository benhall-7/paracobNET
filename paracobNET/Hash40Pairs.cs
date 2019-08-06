using System;
using System.Collections.Generic;

namespace paracobNET
{
    [Serializable]
    public class Hash40Pairs<T> : List<HashValuePair<T>>
    {
        public Hash40Pairs() : base() { }
        public Hash40Pairs(int capacity) : base(capacity) { }

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
        public T this[string keyName, IDictionary<string, ulong> labels]
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
        public T this[ulong hash]
        {
            get
            {
                foreach (var pair in this)
                {
                    if (pair.Key == hash)
                        return pair.Value;
                }
                throw new KeyNotFoundException($"The given key '{Hash40Util.FormatToString(hash)}' was not present in the dictionary.");
            }
            set
            {
                foreach (var pair in this)
                {
                    if (pair.Key == hash)
                    {
                        pair.Value = value;
                        return;
                    }
                }
                throw new KeyNotFoundException($"The given key '{Hash40Util.FormatToString(hash)}' was not present in the dictionary.");
            }
        }

        public IEnumerable<T> IndexEnumerable(string keyName)
        {
            try { return IndexEnumerable(Hash40Util.StringToHash40(keyName)); }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"The given key '{keyName}' was not present in the dictionary.");
            }
        }
        public IEnumerable<T> IndexEnumerable(string keyName, IDictionary<string, ulong> labels)
        {
            try { return IndexEnumerable(Hash40Util.LabelToHash40(keyName, labels)); }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"The given key '{keyName}' was not present in the dictionary.");
            }
        }
        public IEnumerable<T> IndexEnumerable(ulong hash)
        {
            int count = 0;
            foreach (var pair in this)
            {
                if (pair.Key == hash)
                {
                    count++;
                    yield return pair.Value;
                }
            }
            if (count == 0)
                throw new KeyNotFoundException($"The given key '{Hash40Util.FormatToString(hash)}' was not present in the dictionary.");
        }

        public bool ContainsKey(string keyName)
        {
            return ContainsKey(Hash40Util.StringToHash40(keyName));
        }
        public bool ContainsKey(string keyName, IDictionary<string, ulong> labels)
        {
            return ContainsKey(Hash40Util.LabelToHash40(keyName, labels));
        }
        public bool ContainsKey(ulong hash)
        {
            foreach (var pair in this)
            {
                if (pair.Key == hash)
                    return true;
            }
            return false;
        }

        public void Add(string keyName, T value)
        {
            try { Add(Hash40Util.StringToHash40(keyName), value); }
            catch (ArgumentException)
            {
                throw new ArgumentException($"An item with the key '{keyName}' has already been added");
            }
        }
        public void Add(string keyName, IDictionary<string, ulong> labels, T value)
        {
            try { Add(Hash40Util.LabelToHash40(keyName, labels), value); }
            catch (ArgumentException)
            {
                throw new ArgumentException($"An item with the key '{keyName}' has already been added");
            }
        }
        public void Add(ulong key, T value)
        {
            Add(new HashValuePair<T>(key, value));
        }
    }
}
