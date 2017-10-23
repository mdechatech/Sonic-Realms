using System;
using System.Collections.Generic;

namespace SonicRealms.Core.Internal
{
    public class SrMap<T1, T2>
    {
        private readonly Dictionary<T1, T2> forward;
        private readonly Dictionary<T2, T1> reverse;

        public SrMap()
        {
            this.forward = new Dictionary<T1, T2>();
            this.reverse = new Dictionary<T2, T1>();

            this.Forward = new Indexer<T1, T2>(forward);
            this.Reverse = new Indexer<T2, T1>(reverse);
        }

        public void Add(T1 t1, T2 t2)
        {
            if (forward.ContainsKey(t1)) throw new ArgumentException("Duplicate forward key.");
            if (reverse.ContainsKey(t2)) throw new ArgumentException("Duplicate reverse key.");

            forward.Add(t1, t2);
            reverse.Add(t2, t1);
        }

        public void Clear()
        {
            forward.Clear();
            reverse.Clear();
        }

        public bool RemoveForward(T1 forwardKey)
        {
            if (forward.ContainsKey(forwardKey))
            {
                reverse.Remove(forward[forwardKey]);
                return forward.Remove(forwardKey);
            }

            return false;
        }

        public bool RemoveReverse(T2 reverseKey)
        {
            if (reverse.ContainsKey(reverseKey))
            {
                forward.Remove(reverse[reverseKey]);
                return reverse.Remove(reverseKey);
            }

            return false;
        }

        public Indexer<T1, T2> Forward { get; private set; }

        public Indexer<T2, T1> Reverse { get; private set; }

        public int Count { get { return forward.Count; } }

        public class Indexer<T3, T4>
        {
            private readonly Dictionary<T3, T4> dictionary;

            public Indexer(Dictionary<T3, T4> dictionary)
            {
                this.dictionary = dictionary;
            }

            public ICollection<T3> Keys { get { return dictionary.Keys; } }

            public ICollection<T4> Values { get { return dictionary.Values; } }

            public T4 this[T3 index] { get { return dictionary[index]; } set { dictionary[index] = value; } }

            public bool ContainsKey(T3 key)
            {
                return dictionary.ContainsKey(key);
            }
        }
    }
}
