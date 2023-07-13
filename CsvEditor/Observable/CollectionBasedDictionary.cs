using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace CsvEditor.Observable
{
    [DebuggerDisplay("Count = {Count}")]
    public partial class CollectionBasedDictionary<TKey, TValue> :
        KeyedCollection<TKey, KeyValuePair<TKey, TValue>>,
        IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary
    {
        public CollectionBasedDictionary() { }
        public CollectionBasedDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public CollectionBasedDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }
        public CollectionBasedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(comparer)
        {
            foreach (var kvp in dictionary)
            {
                Add(kvp);
            }
        }

        public new TValue this[TKey key]
        {
            get => base[key].Value;
            set
            {
                var newKvp = new KeyValuePair<TKey, TValue>(key, value);
                if (TryGetValueInternal(key, out var oldKvp))
                {
                    SetItem(IndexOf(oldKvp), newKvp);
                }
                else
                {
                    Add(newKvp);
                }
            }
        }

        object IDictionary.this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value;
        }

        public KeyCollection Keys => _keys ?? (_keys = new KeyCollection(this));
        private KeyCollection _keys;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;
        ICollection IDictionary.Keys => this.Keys;

        public ValueCollection Values => _values ?? (_values = new ValueCollection(this));
        private ValueCollection _values;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => this.Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;
        ICollection IDictionary.Values => this.Values;

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item) => item.Key;

        public void Add(TKey key, TValue value) => base.Add(new KeyValuePair<TKey, TValue>(key, value));
        void IDictionary.Add(object key, object value) => this.Add((TKey)key, (TValue)value);

        void IDictionary.Remove(object key) => this.Remove((TKey)key);

        public bool ContainsKey(TKey key) => Dictionary?.ContainsKey(key) ?? false;
        bool IDictionary.Contains(object key) => this.ContainsKey((TKey)key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            var ret = TryGetValueInternal(key, out var kvp);
            value = kvp.Value;
            return ret;
        }

        protected bool TryGetValueInternal(TKey key, out KeyValuePair<TKey, TValue> value)
        {
            if (Dictionary == null)
            {
                value = default(KeyValuePair<TKey, TValue>);
                return false;
            }
            return Dictionary.TryGetValue(key, out value);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(this.GetEnumerator());
        }

        #region Nested Types
        public class KeyCollection : ICollection<TKey>, IReadOnlyList<TKey>, ICollection
        {
            public KeyCollection(CollectionBasedDictionary<TKey, TValue> source)
            {
                _source = source ?? throw new NullReferenceException(nameof(source));
            }

            private CollectionBasedDictionary<TKey, TValue> _source;

            public TKey this[int index] => _source[index].Key;

            public int Count => _source.Count;

            public bool IsReadOnly => true;

            public object SyncRoot { get; } = new object();

            public bool IsSynchronized => false;

            public void Add(TKey item) => throw new NotSupportedException();

            public bool Remove(TKey item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TKey item) => _source.ContainsKey(item);

            public void CopyTo(TKey[] array, int arrayIndex) => _source
                .Select(x => x.Value)
                .ToArray()
                .CopyTo(array, arrayIndex);
            void ICollection.CopyTo(Array array, int index) => this.CopyTo((TKey[])array, index);

            public IEnumerator<TKey> GetEnumerator() => _source.Select(x => x.Key).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        public class ValueCollection : ICollection<TValue>, IReadOnlyList<TValue>, ICollection
        {
            public ValueCollection(CollectionBasedDictionary<TKey, TValue> source)
            {
                _source = source ?? throw new NullReferenceException(nameof(source));
            }

            private CollectionBasedDictionary<TKey, TValue> _source;

            public TValue this[int index] => _source[index].Value;

            public int Count => _source.Count;

            public bool IsReadOnly => true;

            public object SyncRoot { get; } = new object();

            public bool IsSynchronized => false;

            public void Add(TValue item) => throw new NotSupportedException();

            public bool Remove(TValue item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TValue item) => _source
                .Select(x => x.Value)
                .Contains(item);

            public void CopyTo(TValue[] array, int arrayIndex) => _source
                .Select(x => x.Value)
                .ToArray()
                .CopyTo(array, arrayIndex);
            void ICollection.CopyTo(Array array, int index) => this.CopyTo((TValue[])array, index);

            public IEnumerator<TValue> GetEnumerator() => _source.Select(x => x.Value).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        public class DictionaryEnumerator : IDictionaryEnumerator, IDisposable
        {
            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> source)
            {
                _source = source ?? throw new ArgumentNullException(nameof(source));
            }

            private IEnumerator<KeyValuePair<TKey, TValue>> _source;

            public object Key => _source.Current.Key;

            public object Value => _source.Current.Value;

            public DictionaryEntry Entry => new DictionaryEntry(Key, Value);

            public object Current => _source.Current;

            public bool MoveNext() => _source.MoveNext();

            public void Reset() => _source.Reset();

            #region IDisposable Support
            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        (_source as IDisposable)?.Dispose();
                    }
                    disposedValue = true;
                }
            }

            public void Dispose() => Dispose(true);
            #endregion
        }
        #endregion
    }
}
